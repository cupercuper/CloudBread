using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;

using System.Threading.Tasks;
using System.Diagnostics;
using Logger.Logging;
using CloudBread.globals;
using CloudBreadLib.BAL.Crypto;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using Newtonsoft.Json;
using CloudBreadAuth;
using System.Security.Claims;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using CloudBread.Models;
using System.IO;
using DW.CommonData;
using StackExchange.Redis;
using CloudBreadRedis;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWRankController : ApiController
    {
        // GET api/DWRank
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRankInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRankInputParams>(decrypted);

                }
                catch (Exception ex)
                {
                    ex = (Exception)Activator.CreateInstance(ex.GetType(), "Decrypt Error", ex);
                    throw ex;
                }
            }

            // Get the sid or memberID of the current user.
            string sid = CBAuth.getMemberID(p.memberID, this.User as ClaimsPrincipal);
            p.memberID = sid;

            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            string jsonParam = JsonConvert.SerializeObject(p);

            HttpResponseMessage response = new HttpResponseMessage();
            EncryptedData encryptedResult = new EncryptedData();

            try
            {
                DWRankModel result = GetResult(p);

                /// Encrypt the result response
                if (globalVal.CloudBreadCryptSetting == "AES256")
                {
                    try
                    {
                        encryptedResult.token = Crypto.AES_encrypt(JsonConvert.SerializeObject(result), globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                        response = Request.CreateResponse(HttpStatusCode.OK, encryptedResult);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        ex = (Exception)Activator.CreateInstance(ex.GetType(), "Encrypt Error", ex);
                        throw ex;
                    }
                }

                response = Request.CreateResponse(HttpStatusCode.OK, result);
                return response;
            }

            catch (Exception ex)
            {
                // error log
                logMessage.memberID = p.memberID;
                logMessage.Level = "ERROR";
                logMessage.Logger = "DWRankController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        const int MAX_COUNT = 20;
        DWRankModel GetResult(DWRankInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRankModel result = new DWRankModel();
            result.rankList = new List<DWRankData>();

            result.rankCnt = CBRedis.GetRankCount((int)p.rankType);
            // rank는 0 부터 시작한다.
            long myRank = CBRedis.GetSortedSetRank((int)p.rankType, p.memberID);
            double myScore = CBRedis.GetSortedSetScore((int)p.rankType, p.memberID);

            result.myRankData = new DWRankData()
            {
                memberID = p.memberID,
                rank = myRank + 1,
                score = myScore
            };

            long firstIndex = 0;
            long lastIndex = 0;

            if(p.rankSortType == (byte)RANK_SORT_TYPE.TOP_RANK_TYPE || result.rankCnt <= 20)
            {
                lastIndex = 19;
            }
            else
            {
                firstIndex = myRank - 9;
                lastIndex = myRank + 10;

                long firstGap = 0;
                if (firstIndex < 0)
                {
                    firstGap = -firstIndex;
                    firstIndex = 0;
                }

                long lastGap = 0;
                if (lastIndex > result.rankCnt - 1)
                {
                    lastGap = lastIndex - (result.rankCnt - 1);
                    lastIndex = result.rankCnt - 1;
                }

                if (firstGap > 0)
                {
                    lastIndex = lastIndex + firstGap;
                }
                else if (lastGap > 0)
                {
                    firstIndex = firstIndex - lastGap;
                }
            }


            SortedSetEntry[] sortedSetRank = CBRedis.GetSortedSetRankByRange((int)p.rankType, firstIndex, lastIndex);
            Dictionary<string, string> userNickNameDic = new Dictionary<string, string>();
            string strQuery = string.Format("SELECT MemberID, NickName FROM[dbo].[DWMembers] Where MemberID IN (");
            for(int i = 0; i < sortedSetRank.Length; ++i)
            {
                if (i == sortedSetRank.Length - 1)
                {
                    strQuery += string.Format("'{0}'", sortedSetRank[i].Element);
                }
                else
                {
                    strQuery += string.Format("'{0}',", sortedSetRank[i].Element);
                }
            }

            strQuery += ")";

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWRankController";
                            logMessage.Message = string.Format("Not Found User strQuery = {0}", strQuery);
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            string memberID = (string)dreader[0];
                            string nickName = (string)dreader[1];

                            userNickNameDic.Add(memberID, nickName);
                        }
                    }
                }
            }

            for(int i = 0; i < sortedSetRank.Length; ++i)
            {
                string nickName = string.Empty;
                if (userNickNameDic.TryGetValue(sortedSetRank[i].Element, out nickName) == false)
                {
                    continue;
                }

                DWRankData rankData = new DWRankData()
                {
                    memberID = sortedSetRank[i].Element,
                    nickName = nickName,
                    rank = (firstIndex + 1) + i,
                    score = sortedSetRank[i].Score
                };
                result.rankList.Add(rankData);
            }

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
