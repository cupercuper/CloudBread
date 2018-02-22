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
using CloudBreadRedis;


namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWChangeCaptianController : ApiController
    {
        // GET api/DWChangeCaptian
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWChangeCaptianInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWChangeCaptianInputParam>(decrypted);

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
                DWChangeCaptianModel result = GetResult(p);

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
                logMessage.Logger = "DWChangeCaptianController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWChangeCaptianModel GetResult(DWChangeCaptianInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWChangeCaptianModel result = new DWChangeCaptianModel();

            short lastWorld = 0;
            long enhancedStone = 0;
            long cashEnhancedStone = 0;
            long captianChange = 0;
            bool allClear = false;

            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT EnhancedStone, CashEnhancedStone, CaptianChange, LastWorld, AllClear FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWChangeCaptianController";
                            logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            return result;
                        }

                        while (dreader.Read())
                        {
                            enhancedStone = (long)dreader[0];
                            cashEnhancedStone = (long)dreader[1];
                            captianChange = (long)dreader[2];
                            lastWorld = (short)dreader[3];
                            allClear = (bool)dreader[4];
                        }
                    }
                }
            }

            if(lastWorld <= 1)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWChangeCaptianController";
                logMessage.Message = string.Format("Dont CaptainChange MemberID = {0}, LastWorld = {1}", p.memberID, lastWorld);
                Logging.RunLog(logMessage);

                return result;
            }

            WorldDataTable worldDataTable = allClear == true ? DWDataTableManager.GetDataTable(WorldDataTable_List.NAME, (ulong)lastWorld) as WorldDataTable : DWDataTableManager.GetDataTable(WorldDataTable_List.NAME, (ulong)lastWorld - 1) as WorldDataTable;
            if(worldDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWChangeCaptianController";
                logMessage.Message = string.Format("WorldDataTable = null MemberID = {0}, LastWorld = {1}, AllClear = {2}", p.memberID, lastWorld, allClear);
                Logging.RunLog(logMessage);

                return result;
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWChangeCaptianController";
            
            DWMemberData.AddEnhancedStone(ref enhancedStone, ref cashEnhancedStone, worldDataTable.EnhancementStone, 0, logMessage);

            Logging.RunLog(logMessage);

            if (captianChange < long.MaxValue)
            {
                captianChange++;
            }

            byte captianID = DWDataTableManager.GetCaptianID();
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            { 
                string strQuery = string.Format("UPDATE DWMembers SET CaptianID = @captianID, CaptianLevel = @captianLevel, CaptianChange = @captianChange, EnhancedStone = @enhancedStone, CurWorld = @curWorld, LastWorld = @lastWorld, CurStage = @curStage, LastStage = @lastStage WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = captianID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@captianChange", SqlDbType.BigInt).Value = captianChange;
                    command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@lastStage", SqlDbType.SmallInt).Value = 1;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWChangeCaptianController";
                        logMessage.Message = string.Format("Update Failed MemberID = {0}", p.memberID);
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            CBRedis.SetSortedSetRank((int)RANK_TYPE.CUR_STAGE_TYPE, p.memberID, 1);

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWChangeCaptianController";
            logMessage.Message = string.Format("CaptianID = {0}, EnhancedStone = {1}, CaptianChange = {2}", captianID, enhancedStone, captianChange);
            Logging.RunLog(logMessage);

            result.captianID = captianID;
            result.enhancedStone = enhancedStone;
            result.captianChange = captianChange;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
