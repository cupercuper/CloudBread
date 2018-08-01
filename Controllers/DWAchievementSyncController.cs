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
using CloudBread.Manager;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWAchievementSyncController : ApiController
    {
        // GET api/DWAchievementSync
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWAchievementSyncInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWAchievementSyncInputParam>(decrypted);

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
                DWAchievementSyncModel result = result = GetResult(p);

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
                logMessage.Logger = "DWAchievementSyncController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWAchievementSyncModel GetResult(DWAchievementSyncInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWAchievementSyncModel result = new DWAchievementSyncModel();

            List<QuestData> achievementList = null;

            // Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT AchievementList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWAchievementSyncController";
                            logMessage.Message = string.Format("Select Failed");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            achievementList = DWMemberData.ConvertQuestDataList(dreader[0] as byte[]);
                        }
                    }
                }
            }
            
            for(int i = 0; i < achievementList.Count; ++i)
            {
                QuestData questData = p.achievementSyncList.Find(syncItem => syncItem.serialNo == achievementList[i].serialNo);
                if(questData == null)
                {
                    continue;
                }

                achievementList[i].curValue = questData.curValue;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET AchievementList = @achievementList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@achievementList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(achievementList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWAchievementSyncController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
