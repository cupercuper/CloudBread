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
    public class DWGetMailController : ApiController
    {
        // GET api/DWGetMail
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWGetMailInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWGetMailInputParam>(decrypted);

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
                DWGetMailModel result = result = GetResult(p);

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

        const double LIMIT_DAY = 30.0;
        DWGetMailModel GetResult(DWGetMailInputParam p)
        {
            DateTime utcTime = DateTime.UtcNow;
            utcTime = utcTime.AddDays(-LIMIT_DAY);

            DWGetMailModel result = new DWGetMailModel();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("WITH TEMPTABLE AS(SELECT ROW_NUMBER() OVER(ORDER BY CreatedAt) AS rownum, [Index], SenderID, ReceiveID, MailData, CreatedAt FROM DWMail WHERE CreatedAt >= @createdAt AND [Read] = 0 AND ReceiveID = @receiveID) SELECT[Index], SenderID, ReceiveID, MailData, CreatedAt FROM TEMPTABLE WHERE rownum > @start AND rownum <= @last ORDER BY rownum");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@createdAt", SqlDbType.DateTime).Value = utcTime;
                    command.Parameters.Add("@receiveID", SqlDbType.NVarChar).Value = p.memberID;                    
                    command.Parameters.Add("@start", SqlDbType.Int).Value = p.startIndex;
                    command.Parameters.Add("@last", SqlDbType.Int).Value = p.startIndex + p.offset;

                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        while (dreader.Read())
                        {
                            long index = (long)dreader[0];
                            string senderID = (string)dreader[1];
                            string receiveID = (string)dreader[2];
                            DWMailData mailData = DWMemberData.ConvertMailData(dreader[3] as byte[]);
                            DateTime createdAt = (DateTime)dreader[4];

                            mailData.index = index;
                            mailData.senderID = senderID;
                            mailData.receiveID = receiveID;
                            mailData.createdAt = createdAt;
                        
                            result.mailList.Add(mailData);
                        }

                        result.errorCode = (byte)DW_ERROR_CODE.OK;
                    }
                }
            }

            return result;
        }

    }
}
