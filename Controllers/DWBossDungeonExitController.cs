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
    public class DWBossDungeonExitController : ApiController
    {
        // GET api/DWBossDungeonExit
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBossDungeonExitInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBossDungeonExitInputParam>(decrypted);

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
                DWBossDungeonExitModel result = result = GetResult(p);

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
                logMessage.Logger = "DWBossDungeonExitController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWBossDungeonExitModel GetResult(DWBossDungeonExitInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWBossDungeonExitModel result = new DWBossDungeonExitModel();

            long gem = 0;
            long enhancedStone = 0;
            long cashGem = 0;
            long cashEnhancedStone = 0;
            long gold = 0;
            short lastBossDungeonNo = 0;
            byte bossDungeonEnterType = 0;
            int bossDungeonTicket = 0;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone, LastBossDungeonNo, BossDungeonEnterType, BossDungeonTicket FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWBossDungeonExitController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gold = (long)dreader[0];
                            gem = (long)dreader[1];
                            cashGem = (long)dreader[2];
                            enhancedStone = (long)dreader[3];
                            cashEnhancedStone = (long)dreader[4];
                            lastBossDungeonNo = (short)dreader[5];
                            bossDungeonEnterType = (byte)dreader[6];
                            bossDungeonTicket = (int)dreader[7];
                        }
                    }
                }
            }

            if((BOSS_DUNGEON_ENTER_TYPE)bossDungeonEnterType == BOSS_DUNGEON_ENTER_TYPE.MAX_TYPE)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBossDungeonExitController";
                logMessage.Message = string.Format("BossDungeonEnterType  Error");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if((BOSS_DUNGEON_ENTER_TYPE)bossDungeonEnterType == BOSS_DUNGEON_ENTER_TYPE.NORMAL_ENTER_TYPE)
            {
                if(bossDungeonTicket <= 0)
                {
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWBossDungeonExitController";
                    logMessage.Message = string.Format("Lack Boss Dungeon Ticket  Error");
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                --bossDungeonTicket;
            }
            else if((BOSS_DUNGEON_ENTER_TYPE)bossDungeonEnterType == BOSS_DUNGEON_ENTER_TYPE.GEM_ENTER_TYPE)
            {
                logMessage.memberID = p.memberID;
                if (DWMemberData.SubGem(ref gem, ref cashGem, DWDataTableManager.GlobalSettingDataTable.BossDugeonAddMoney, logMessage) == false)
                {
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWBossDungeonExitController";
                    logMessage.Message = string.Format("Lack Gem  Error");
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
                logMessage.Level = "Info";
                logMessage.Logger = "DWBossDungeonExitController";
                Logging.RunLog(logMessage);

            }

            long addGold = 0;
            if (p.clear == 1)
            {
                if(p.curBossDungeonNo == lastBossDungeonNo)
                {
                    ++lastBossDungeonNo;
                    BossDungeonDataTable bossDungeonDataTableList = DWDataTableManager.GetDataTable(BossDungeonDataTable_List.NAME, (ulong)lastBossDungeonNo) as BossDungeonDataTable;
                    if(bossDungeonDataTableList == null)
                    {
                        --lastBossDungeonNo;
                    }
                }

                BossDungeonDataTable clearBossDungeonDataTableList = DWDataTableManager.GetDataTable(BossDungeonDataTable_List.NAME, (ulong)p.curBossDungeonNo) as BossDungeonDataTable;
                if(clearBossDungeonDataTableList == null)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWBossDungeonExitController";
                    logMessage.Message = string.Format("Not Found BossDungeonDataTable SerialNo = {0}", p.curBossDungeonNo);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                DWMemberData.AddEnhancedStone(ref enhancedStone, ref cashEnhancedStone, clearBossDungeonDataTableList.EnhancementStone, 0, logMessage);
                logMessage.memberID = p.memberID;
                logMessage.Level = "Info";
                logMessage.Logger = "DWBossDungeonExitController";
                Logging.RunLog(logMessage);

                addGold = clearBossDungeonDataTableList.Gold;
                gold += clearBossDungeonDataTableList.Gold;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET Gold = @gold, Gem = @gem, CashGem = @cashGem, EnhancedStone = @enhancedStone, CashEnhancedStone = @cashEnhancedStone, LastBossDungeonNo = @lastBossDungeonNo, BossDungeonEnterType = @bossDungeonEnterType, BossDungeonTicket = @bossDungeonTicket WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gold", SqlDbType.BigInt).Value = gold;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                    command.Parameters.Add("@cashEnhancedStone", SqlDbType.BigInt).Value = cashEnhancedStone;
                    command.Parameters.Add("@lastBossDungeonNo", SqlDbType.SmallInt).Value = lastBossDungeonNo;
                    command.Parameters.Add("@bossDungeonEnterType", SqlDbType.TinyInt).Value = (byte)BOSS_DUNGEON_ENTER_TYPE.MAX_TYPE;
                    command.Parameters.Add("@bossDungeonTicket", SqlDbType.Int).Value = bossDungeonTicket;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWBossDungeonExitController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "Info";
            logMessage.Logger = "DWBossDungeonExitController";
            logMessage.Message = string.Format("BossDungeon Exit Clear = {0}, DungeonNo = {1}", p.clear, p.curBossDungeonNo);
            Logging.RunLog(logMessage);

            result.addGold = addGold;
            result.gem = gem;
            result.cashGem = cashGem;
            result.enhancedStone = enhancedStone;
            result.lastBossDungeonNo = lastBossDungeonNo;
            result.bossDungeonTicket = bossDungeonTicket;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
