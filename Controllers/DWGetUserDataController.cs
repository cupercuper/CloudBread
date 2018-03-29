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
    public class DWGetUserDataController : ApiController
    {
        // GET api/DWGetUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWGetUserDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWGetUserDataInputParams>(decrypted);

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
                DWGetUserDataModel result = result = GetResult(p);

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

        DWGetUserDataModel GetResult(DWGetUserDataInputParams p)
        {
            DWGetUserDataModel result = new DWGetUserDataModel();
            DateTime bossDungeonTicketRefreshTime = DateTime.UtcNow;

            // Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                //string strQuery = string.Format("SELECT MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, CaptianChange, LastWorld, CurWorld, CurStage, UnitList, CanBuyUnitList, Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitListChangeTime, UnitStore, UnitStoreList, AllClear, ActiveItemList, LimitShopItemDataList, UnitTicketList, LastStage, AccStageCnt, BossDungeonTicket, LastBossDungeonNo, BossDungeonTicketRefreshTime FROM DWMembers WHERE MemberID = '{0}'", p.memberID);

                string strQuery = string.Format("SELECT MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, CaptianChange, LastWorld, CurWorld, CurStage, UnitList, CanBuyUnitList, Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitListChangeTime, AllClear, ActiveItemList, LimitShopItemDataList, UnitTicketList, LastStage, AccStageCnt, BossDungeonTicket, LastBossDungeonNo, BossDungeonTicketRefreshTime, UnitDeckList, BossClearList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            Dictionary<uint, UnitData> unitDic = DWMemberData.ConvertUnitDic(dreader[9] as byte[]); // UnitList
                            DWUserData workItem = new DWUserData()
                            {
                                memberID = dreader[0].ToString(), // MemberID
                                nickName = dreader[1].ToString(), // NickName
                                recommenderID = dreader[2].ToString(), // RecommenderID
                                captianLevel = (short)dreader[3], // CaptianLevel
                                captianID = (byte)dreader[4], // CaptianID
                                captianChange = (long)dreader[5], // CaptianChange
                                lastWorld = (short)dreader[6], // LastWorld
                                curWorld = (short)dreader[7], // CurWorld
                                curStage = (short)dreader[8], // CurStage
                                unitList = DWMemberData.ConvertClientUnitData(unitDic),
                                canBuyUnitList = DWMemberData.ConvertUnitList(dreader[10] as byte[]), // CanBuyUnitList
                                gold = (long)dreader[11], // Gold
                                gem = (long)dreader[12], // Gem
                                cashGem = (long)dreader[13], // CashGem
                                enhancedStone = (long)dreader[14], // EnhancedStone
                                cashEnhancedStone = (long)dreader[15], // CashEnhancedStone
                                unitSlotIdx = (byte)dreader[16], // UnitSlotIdx
                                unitListChangeTime = ((DateTime)dreader[17]).Ticks, // UnitListChangeTime
                                //unitStore = (byte)dreader[18],
                                //unitStoreList = DWMemberData.ConvertUnitStoreList(dreader[19] as byte[]),
                                allClear = (bool)dreader[18], //AllClear
                                activeItemList = DWMemberData.ConvertActiveItemList(dreader[19] as byte[]), // ActiveItemList
                                limitShopItemDataList = DWMemberData.ConvertLimitShopItemDataList(dreader[20] as byte[]), // LimitShopItemDataList
                                unitTicketList = DWMemberData.ConvertUnitTicketDataList(dreader[21] as byte[]), // UnitTicketList
                                lastStage = (short)dreader[22], // LastStage
                                accStage = (long)dreader[23], // AccStageCnt
                                bossDungeonTicket = (int)dreader[24], // BossDungeonTicket
                                lastBossDungeonNo = (short)dreader[25], // LastBossDungeonNo
                                unitDeckList = DWMemberData.ConvertUnitDeckList(dreader[27] as byte[]), // UnitDeckList
                                bossClearList = DWMemberData.ConvertBossClearList(dreader[28] as byte[])
                            };

                            bossDungeonTicketRefreshTime = (DateTime)dreader[26];

                            result.userDataList.Add(workItem);
                            result.errorCode = (byte)DW_ERROR_CODE.OK;
                        }
                    }
                }
            }


            int KOREA_TIME_ZONE = 9;
            DWMemberData.BossDungeonTicketRefresh(ref bossDungeonTicketRefreshTime, ref result.userDataList[0].bossDungeonTicket, KOREA_TIME_ZONE, DWDataTableManager.GlobalSettingDataTable.BossDugeonTicketCount);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET GemBoxGet = @gemBoxGet, BossDungeonTicketRefreshTime = @bossDungeonTicketRefreshTime, BossDungeonTicket = @bossDungeonTicket, BossDungeonEnterType = @bossDungeonEnterType WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gemBoxGet", SqlDbType.Bit).Value = true;
                    command.Parameters.Add("@bossDungeonTicketRefreshTime", SqlDbType.DateTime).Value = bossDungeonTicketRefreshTime;
                    command.Parameters.Add("@bossDungeonTicket", SqlDbType.Int).Value = result.userDataList[0].bossDungeonTicket;
                    command.Parameters.Add("@bossDungeonEnterType", SqlDbType.TinyInt).Value = (byte)BOSS_DUNGEON_ENTER_TYPE.MAX_TYPE;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            DWMemberData.UpdateActiveItem(result.userDataList[0].activeItemList);

            return result;
        }
    }
}
