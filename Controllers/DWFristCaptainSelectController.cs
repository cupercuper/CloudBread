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
    public class DWFristCaptainSelectController : ApiController
    {
        // GET api/DWFristCaptainSelect
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWFristCaptainSelectDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWFristCaptainSelectDataInputParam>(decrypted);

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
                DWFristCaptainSelectDataModel result = GetResult(p);

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

        DWFristCaptainSelectDataModel GetResult(DWFristCaptainSelectDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWFristCaptainSelectDataModel result = new DWFristCaptainSelectDataModel();

            byte captainID = 0;
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT CaptianID FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            captainID = (byte)dreader[0];
                        }
                    }
                }
            }

            if (captainID != 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            CaptianDataTable captainDataTable = DWDataTableManager.GetDataTable(CaptianDataTable_List.NAME, p.captainID) as CaptianDataTable;
            if(captainDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET CaptianID = @captianID, CaptianLevel = @captianLevel WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = p.captainID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = 1;

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

            result.captainID = p.captainID;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
