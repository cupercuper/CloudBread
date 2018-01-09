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
    public class DWEventController : ApiController
    {
        // GET api/DWEvent
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWEventInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWEventInputParams>(decrypted);

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
                DWEventModel result = GetResult(p);

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
                logMessage.Logger = "DWEventController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWEventModel GetResult(DWEventInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWEventModel result = new DWEventModel();

            long index = 0;
            EventData eventData = null;
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT [Index], EventData FROM DWEvent WHERE EventType = @eventType AND StartTime <= @currentTime AND EndTime >= @currentTime");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@eventType", SqlDbType.TinyInt).Value = p.eventCheckType;
                    command.Parameters.Add("@currentTime", SqlDbType.DateTime).Value = DateTime.UtcNow;

                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        while (dreader.Read())
                        {
                            index = (long)dreader[0];
                            eventData = DWMemberData.ConvertEventData(dreader[1] as byte[]);
                        }
                    }
                }
            }

            if(eventData == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            List<long> eventList = new List<long>();
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT EventList FROM DWMembersInputEvent WHERE MemberID = @memberID");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = p.memberID;

                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        while (dreader.Read())
                        {
                            eventList = DWMemberData.ConvertEventList(dreader[0] as byte[]);
                        }
                    }
                }
            }

            if(eventList.Contains(index))
            {
                result.errorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            eventList.Add(index);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersInputEvent SET EventList = @eventList WHERE MemberID = @memberID");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@eventList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(eventList);
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = p.memberID;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWEventController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            DWMailData mailData = new DWMailData();
            mailData.title = eventData.title;
            mailData.msg = eventData.msg;
            mailData.itemData = new List<DWItemData>();
            for(int i = 0; i < eventData.itemData.Count; ++i)
            {
                mailData.itemData.Add(eventData.itemData[i]);
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "Insert into DWMail (SenderID, ReceiveID, MailData) VALUES (@senderID, @receiveID, @mailData)";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@senderID", SqlDbType.NVarChar).Value = "Master";
                    command.Parameters.Add("@receiveID", SqlDbType.NVarChar).Value = p.memberID;
                    command.Parameters.Add("@mailData", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(mailData);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWEventController";
                        logMessage.Message = string.Format("Insert Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWEventController";
            logMessage.Message = string.Format("Event Index = {0}", index);
            Logging.RunLog(logMessage);

            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
