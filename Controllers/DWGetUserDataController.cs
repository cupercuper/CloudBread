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



namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWGetUserDataController : ApiController
    {
        // GET api/DWGetUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWGetUserDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWGetUserDataInputParams>(decrypted);

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
                DWGetUserDataModel result = result = GetResult(p);

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
                logMessage.Logger = "DWGetUserDataController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWGetUserDataModel GetResult(DWGetUserDataInputParams p)
        {
            DWGetUserDataModel result = new DWGetUserDataModel();
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, CaptianChange, LastWorld, CurWorld, CurStage, UnitList, CanBuyUnitList, Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitListChangeTime, UnitStore, UnitStoreList, AllClear FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if(dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            Dictionary<uint, UnitData> unitDic = DWMemberData.ConvertUnitDic(dreader[9] as byte[]);
                            DWUserData workItem = new DWUserData()
                            {
                                memberID = dreader[0].ToString(),
                                nickName = dreader[1].ToString(),
                                recommenderID = dreader[2].ToString(),
                                captianLevel = (short)dreader[3],
                                captianID = (byte)dreader[4],
                                captianChange = (long)dreader[5],
                                lastWorld = (short)dreader[6],
                                curWorld = (short)dreader[7],
                                curStage = (short)dreader[8],
                                unitList = DWMemberData.ConvertClientUnitData(unitDic),
                                canBuyUnitList = DWMemberData.ConvertUnitList(dreader[10] as byte[]),
                                gold = (long)dreader[11],
                                gem = (long)dreader[12],
                                cashGem = (long)dreader[13],
                                enhancedStone = (long)dreader[14],
                                cashEnhancedStone = (long)dreader[15],
                                unitSlotIdx = (byte)dreader[16],
                                unitListChangeTime = ((DateTime)dreader[17]).Ticks,
                                unitStore = (byte)dreader[18],
                                unitStoreList = DWMemberData.ConvertUnitStoreList(dreader[19] as byte[]),
                                allClear = (bool)dreader[20]
                            };

                            result.userDataList.Add(workItem);
                            result.errorCode = (byte)DW_ERROR_CODE.OK;
                        }
                    }
                }
            }

            return result;
        }
    }
}
