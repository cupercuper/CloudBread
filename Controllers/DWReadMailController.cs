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
    public class DWReadMailController : ApiController
    {
        // GET api/DWReadMail
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWReadMailInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWReadMailInputParams>(decrypted);

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
                DWReadMailModel result = result = GetResult(p);

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
                logMessage.Logger = "DWReadMailController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }
        
        DWReadMailModel GetResult(DWReadMailInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWReadMailModel result = new DWReadMailModel();

            DWMailData mailData = null;
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "SELECT MailData FROM[dbo].[DWMail] Where ReceiveID = @receiveID AND [Index] = @index AND [Read] = 0";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@receiveID", SqlDbType.NVarChar).Value = p.memberID;
                    command.Parameters.Add("@index", SqlDbType.BigInt).Value = p.index;

                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        while (dreader.Read())
                        {
                            mailData = DWMemberData.ConvertMailData(dreader[0] as byte[]);
                        }
                    }
                }
            }

            if(mailData == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWReadMailController";
                logMessage.Message = string.Format("MailData null Index = {0}", p.index);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                return result;
            }

            long gem = 0;
            long cashGem = 0;
            long gold = 0;
            long enhancedStone = 0;
            long cashEnhancedStone = 0;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWReadMailController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gold = (long)dreader[0];
                            gem = (long)dreader[1];
                            cashGem = (long)dreader[2];
                            enhancedStone = (long)dreader[3];
                            cashEnhancedStone = (long)dreader[4];
                        }
                    }
                }
            }

            int addGold = 0;
            for (int i = 0; i < mailData.itemData.Count; ++i)
            {
                switch ((ITEM_TYPE)mailData.itemData[i].itemType)
                {
                    case ITEM_TYPE.GOLD_TYPE:
                        gold += mailData.itemData[i].count;
                        addGold = mailData.itemData[i].count;
                        break;

                    case ITEM_TYPE.GEM_TYPE:
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWReadMailController";
                        DWMemberData.AddGem(ref gem, ref cashGem, mailData.itemData[i].count, 0, logMessage);
                        Logging.RunLog(logMessage);

                        break;

                    case ITEM_TYPE.ENHANCEDSTONE_TYPE:
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWReadMailController";
                        DWMemberData.AddEnhancedStone(ref enhancedStone, ref cashEnhancedStone, mailData.itemData[i].count, 0, logMessage);
                        Logging.RunLog(logMessage);
                        break;
                }
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET Gold = @gold, Gem = @gem, EnhancedStone = @enhancedStone WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gold", SqlDbType.BigInt).Value = gold;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWReadMailController";
                        logMessage.Message = string.Format("Update Failed DWMembers");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMail SET [Read] = 1, [ReadAt] = @readAt WHERE [Index] = @index");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@readAt", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    command.Parameters.Add("@index", SqlDbType.BigInt).Value = p.index;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWReadMailController";
                        logMessage.Message = string.Format("Update Failed DWMail");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWReadMailController";
            logMessage.Message = string.Format("Cur Gem = {0}, Cur EnhancedStone = {1}, Add Gold = {2}", gem, enhancedStone, addGold);
            Logging.RunLog(logMessage);

            result.index = p.index;
            result.gold = addGold;
            result.gem = gem;
            result.enhancedStone = enhancedStone;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }


}
