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
    public class DWUdtStageInfoController : ApiController
    {
        // GET api/DWUdtStageInfo
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWUdtStageInfoInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWUdtStageInfoInputParams>(decrypted);

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
                DWUdtStageInfoModel result = result = GetResult(p);

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
                logMessage.Logger = "DWUdtStageInfoController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWUdtStageInfoModel GetResult(DWUdtStageInfoInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWUdtStageInfoModel result = new DWUdtStageInfoModel();

            short lastWorld = 0;
            short curWorld = 0;
            long captianChange = 0;
            bool allClear = false;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT LastWorld, CurWorld, CaptianChange, AllClear FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWUdtStageInfoController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            lastWorld = (short)dreader[0];
                            curWorld = (short)dreader[1];
                            captianChange = (long)dreader[2];
                            allClear = (bool)dreader[3];
                        }
                    }
                }
            }

            short checkNum = (short)(p.worldNo - lastWorld);
            if(checkNum > 1)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWUdtStageInfoController";
                logMessage.Message = string.Format("Stage Hack lastWorld = {0}, Input World = {1}", lastWorld, p.worldNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if(p.allClear)
            {
                WorldDataTable worldDataTable = DWDataTableManager.GetDataTable(WorldDataTable_List.NAME, (ulong)(lastWorld + 1)) as WorldDataTable;
                if(worldDataTable != null)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWUdtStageInfoController";
                    logMessage.Message = string.Format("All Clear Hack lastWorld = {0}, Input World = {1}", lastWorld, p.worldNo);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            if(p.worldNo > lastWorld)
            {
                lastWorld = p.worldNo;
                //CBRedis.SetSortedSetRank(p.memberID, DWMemberData.GetPoint(lastWorld, captianChange));
            }

            curWorld = p.worldNo;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET LastWorld = @lastWorld, CurWorld = @curWorld, AllClear = @allClear WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = lastWorld;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = curWorld;
                    command.Parameters.Add("@allClear", SqlDbType.Bit).Value = p.allClear;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWUdtStageInfoController";
                        logMessage.Message = string.Format("Udpate Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.allClear = p.allClear;
            result.worldNo = p.worldNo;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
