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
            int enhancedStone = 0;
            byte captianChange = 0;
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT EnhancedStone, CaptianChange, LastWorld FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWChangeCaptianController";
                            logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            return result;
                        }

                        while (dreader.Read())
                        {
                            enhancedStone = (int)dreader[0];
                            captianChange = (byte)dreader[1];
                            lastWorld = (short)dreader[2];
                        }
                    }
                }
            }

            captianChange = 1;
            byte captianID = DWDataTableManager.GetCaptianID();
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            { 
                string strQuery = string.Format("UPDATE DWMembers SET CaptianID = @captianID, CaptianLevel = @captianLevel, CaptianChange = @captianChange, EnhancedStone = @enhancedStone, CurWorld = @curWorld, LastWorld = @lastWorld WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = captianID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@captianChange", SqlDbType.TinyInt).Value = captianChange;
                    command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = enhancedStone;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = 1;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWChangeCaptianController";
                        logMessage.Message = string.Format("Update Failed MemberID = {0}", p.memberID);
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            CBRedis.SetSortedSetRank(p.memberID, DWMemberData.GetPoint(lastWorld, captianChange));

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
