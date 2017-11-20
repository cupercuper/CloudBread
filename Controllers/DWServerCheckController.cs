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
    public class DWServerCheckController : ApiController
    {
        // GET api/DWServerCheck
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWServerCheckInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWServerCheckInputParam>(decrypted);

                }
                catch (Exception ex)
                {
                    ex = (Exception)Activator.CreateInstance(ex.GetType(), "Decrypt Error", ex);
                    throw ex;
                }
            }

            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            string jsonParam = JsonConvert.SerializeObject(p);


            HttpResponseMessage response = new HttpResponseMessage();
            EncryptedData encryptedResult = new EncryptedData();

            try
            {
                DWServerCheckModel result = GetResult();

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
                logMessage.memberID = "Server Check";
                logMessage.Level = "ERROR";
                logMessage.Logger = "DWServerCheckController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWServerCheckModel GetResult()
        {
            DWServerCheckModel result = new DWServerCheckModel();
            result.serverCheckState = (byte)SERVER_CHECK_TYPE.NOT_TYPE;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            bool inputCheck = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT CheckStartTime, CheckEndTime FROM ServerCheck");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            return result;
                        }

                        while (dreader.Read())
                        {
                            inputCheck = true;
                            startTime = DateTime.Parse(dreader[0].ToString());
                            endTime = DateTime.Parse(dreader[1].ToString());
                        }
                    }
                }
            }

            if(inputCheck)
            {
                DateTime utcTime = DateTime.UtcNow;
                if(startTime <= utcTime && utcTime <= endTime)
                {
                    result.serverCheckState = (byte)SERVER_CHECK_TYPE.CHECKING_TYPE;
                }
                else if(endTime < utcTime)
                {
                    result.serverCheckState = (byte)SERVER_CHECK_TYPE.NOT_TYPE;
                }
                else
                {
                    result.serverCheckState = (byte)SERVER_CHECK_TYPE.REGISTER_TYPE;
                }

                if(result.serverCheckState != (byte)SERVER_CHECK_TYPE.NOT_TYPE)
                {
                    result.startTime = new List<long>
                    {
                        startTime.Ticks
                    };

                    result.endTime = new List<long>()
                    {
                        endTime.Ticks
                    };
                }

            }

            return result;
        }

    }
}
