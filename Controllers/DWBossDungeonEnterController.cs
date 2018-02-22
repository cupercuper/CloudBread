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
    public class DWBossDungeonEnterController : ApiController
    {
        // GET api/DWBossDungeonEnter
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBossDungeonEnterInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBossDungeonEnterInputParam>(decrypted);

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
                DWBossDungeonEnterModel result = result = GetResult(p);

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
                logMessage.Logger = "DWBossDungeonEnterController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWBossDungeonEnterModel GetResult(DWBossDungeonEnterInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            logMessage.memberID = p.memberID;

            DWBossDungeonEnterModel result = new DWBossDungeonEnterModel();

            long gem = 0;
            long cashGem = 0;
            int bossDungeonTicket = 0;
            short lastBossDungeonNo = 0;
            DateTime bossDungeonTicketRefreshTime = DateTime.UtcNow;
            byte bossDungeonEnterType = 0;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, BossDungeonTicket, LastBossDungeonNo, BossDungeonTicketRefreshTime, BossDungeonEnterType FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWBossDungeonEnterController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            bossDungeonTicket = (int)dreader[2];
                            lastBossDungeonNo = (short)dreader[3];
                            bossDungeonTicketRefreshTime = (DateTime)dreader[4];
                            bossDungeonEnterType = (byte)dreader[5];

                        }
                    }
                }
            }

            if((BOSS_DUNGEON_ENTER_TYPE)bossDungeonEnterType != BOSS_DUNGEON_ENTER_TYPE.MAX_TYPE)
            {
                logMessage.Level = "Error";
                logMessage.Logger = "DWBossDungeonEnterController";
                logMessage.Message = string.Format("Boss Dungeon Enter Type Error Type = {0}", bossDungeonEnterType);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            int KOREA_TIME_ZONE = 9;
            DWMemberData.BossDungeonTicketRefresh(ref bossDungeonTicketRefreshTime, ref bossDungeonTicket, KOREA_TIME_ZONE, DWDataTableManager.GlobalSettingDataTable.BossDugeonTicketCount);

            if (p.gemUse == 1)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Info";
                logMessage.Logger = "DWBossDungeonEnterController";

                if (gem + cashGem < DWDataTableManager.GlobalSettingDataTable.BossDugeonAddMoney)
                {
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWBossDungeonEnterController";
                    logMessage.Message = string.Format("lack gem gem = {0}, cashGem = {1}, subGem = {2}", gem, cashGem, DWDataTableManager.GlobalSettingDataTable.BossDugeonAddMoney);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                Logging.RunLog(logMessage);

                bossDungeonEnterType = (byte)BOSS_DUNGEON_ENTER_TYPE.GEM_ENTER_TYPE;
            }
            else if (bossDungeonTicket == 0 && p.gemUse == 0)
            {
                logMessage.Level = "Error";
                logMessage.Logger = "DWBossDungeonEnterController";
                logMessage.Message = string.Format("lack boss dungeon ticket");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            else
            {
                bossDungeonEnterType = (byte)BOSS_DUNGEON_ENTER_TYPE.NORMAL_ENTER_TYPE;
            }

            if (p.curBossDungeonNo > lastBossDungeonNo)
            {
                logMessage.Level = "Error";
                logMessage.Logger = "DWBossDungeonEnterController";
                logMessage.Message = string.Format("Boss Dungeon No Over CurNo = {0}, LastNo = {1}", p.curBossDungeonNo, lastBossDungeonNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET BossDungeonTicket = @bossDungeonTicket, BossDungeonTicketRefreshTime = @bossDungeonTicketRefreshTime, BossDungeonEnterType = @bossDungeonEnterType, GemBoxGet = @gemBoxGet WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@bossDungeonTicket", SqlDbType.Int).Value = bossDungeonTicket;
                    command.Parameters.Add("@bossDungeonTicketRefreshTime", SqlDbType.DateTime).Value = bossDungeonTicketRefreshTime;
                    command.Parameters.Add("@bossDungeonEnterType", SqlDbType.TinyInt).Value = bossDungeonEnterType;
                    command.Parameters.Add("@gemBoxGet", SqlDbType.Bit).Value = true;
                    
                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWBossDungeonEnterController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "Info";
            logMessage.Logger = "DWBossDungeonEnterController";
            logMessage.Message = string.Format("BossDungeon Enter GemUse = {0}, DungeonNo = {1}", p.gemUse, p.curBossDungeonNo);
            Logging.RunLog(logMessage);
            
            result.curBossDungeonNo = p.curBossDungeonNo;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
