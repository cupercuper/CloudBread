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
using CloudBread.Manager;



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
            DateTime dailyQuestAcceptTime = DateTime.UtcNow;
            DateTime resourceDrillStartTime = DateTime.UtcNow;
            DateTime luckySupplyShipLastTime = DateTime.UtcNow;

            // Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, CaptianChange, LastWorld, CurWorld, CurStage, UnitList, Gold, Gem, CashGem, Ether, CashEther, AllClear, ActiveItemList, LimitShopItemDataList, LastStage, AccStageCnt, BossDungeonTicket, LastBossDungeonNo, BossDungeonTicketRefreshTime, BossClearList, TimeZone, TimeZoneID, ContinueAttendanceCnt, ContinueAttendanceNo, AccAttendanceCnt, AccAttendanceNo, DailyQuestList, DailyQuestAcceptTime, AchievementList, ResouceDrillIdx, ResouceDrillStartTime, LuckySupplyShipLastTime, SkillItemList, BoxList, RelicList, RelicStoreList, RelicSlotIdx, Gas, CashGas, BaseCampList, RelicBoxCount, GameSpeedItemCount, GameSpeedItemStartTime, LastReturnStage, BaseCampResetCount, RelicInventorySlotIdx, DroneAdvertisingOff FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            DWUserData workItem = new DWUserData();

                            workItem.memberID = dreader[0].ToString(); // MemberID
                            workItem.nickName = dreader[1].ToString(); // NickName
                            workItem.recommenderID = dreader[2].ToString(); // RecommenderID
                            workItem.captianLevel = (short)dreader[3]; // CaptianLevel
                            workItem.captianID = (byte)dreader[4]; // CaptianID
                            workItem.captianChange = (long)dreader[5]; // CaptianChange
                            workItem.lastWorld = (short)dreader[6]; // LastWorld
                            workItem.curWorld = (short)dreader[7]; // CurWorld
                            workItem.curStage = (short)dreader[8]; // CurStage
                            workItem.unitList = DWMemberData.ConvertUnitDataList(dreader[9] as byte[]);
                            workItem.gold = DWMemberData.ConvertDouble(dreader[10] as byte[]); // Gold
                            workItem.gem = (long)dreader[11]; // Gem
                            workItem.cashGem = (long)dreader[12]; // CashGem
                            workItem.ether = (long)dreader[13]; // Ether
                            workItem.cashEther = (long)dreader[14]; // CashEther
                            workItem.allClear = (bool)dreader[15]; //AllClear
                            workItem.activeItemList = DWMemberData.ConvertActiveItemList(dreader[16] as byte[]); // ActiveItemList
                            workItem.limitShopItemDataList = DWMemberData.ConvertLimitShopItemDataList(dreader[17] as byte[]); // LimitShopItemDataList        
                            workItem.lastStage = (short)dreader[18]; // LastStage
                            workItem.accStage = (long)dreader[19]; // AccStageCnt
                            workItem.bossDungeonTicket = (int)dreader[20]; // BossDungeonTicket
                            workItem.lastBossDungeonNo = (short)dreader[21]; // LastBossDungeonNo
                            bossDungeonTicketRefreshTime = (DateTime)dreader[22];
                            workItem.bossClearList = DWMemberData.ConvertBossClearList(dreader[23] as byte[]);
                            workItem.timeZoneTotalMin = (int)dreader[24];
                            workItem.timeZoneID = dreader[25].ToString();
                            workItem.continueAttendanceCnt = (byte)dreader[26];
                            workItem.continueAttendanceNo = (short)dreader[27];
                            workItem.accAttendanceCnt = (byte)dreader[28];
                            workItem.accAttendanceNo = (short)dreader[29];
                            workItem.dailyQuestList = DWMemberData.ConvertQuestDataList(dreader[30] as byte[]);
                            dailyQuestAcceptTime = (DateTime)dreader[31];
                            workItem.achievementList = DWMemberData.ConvertQuestDataList(dreader[32] as byte[]);
                            workItem.resourceDrillIdx = (byte)dreader[33];
                            resourceDrillStartTime = (DateTime)dreader[34];
                            luckySupplyShipLastTime = (DateTime)dreader[35];
                            workItem.skillItemList = DWMemberData.ConvertSkillItemList(dreader[36] as byte[]);
                            workItem.boxList = DWMemberData.ConvertBoxDataList(dreader[37] as byte[]);
                            Dictionary<uint, RelicData> relicDic = DWMemberData.ConvertRelicDataDic(dreader[38] as byte[]);
                            workItem.relicList = new List<RelicData>();
                            workItem.relicList.AddRange(relicDic.Values.ToArray());
                            Dictionary<uint, RelicData> relicStoreDic = DWMemberData.ConvertRelicDataDic(dreader[39] as byte[]);
                            workItem.relicStoreList = new List<RelicData>();
                            workItem.relicStoreList.AddRange(relicStoreDic.Values.ToArray());
                            workItem.relicSlotIdx = (byte)dreader[40];
                            workItem.gas = (long)dreader[41];
                            workItem.cashEther = (long)dreader[42];
                            Dictionary<ulong, ushort> baseCampDic = DWMemberData.ConvertBaseCampDic(dreader[43] as byte[]);
                            workItem.baseCampList = DWMemberData.ConvertBaseCampList(baseCampDic);
                            workItem.relicBoxCnt = (long)dreader[44];
                            workItem.gameSpeedItemCnt = (byte)dreader[45];
                            DateTime gameSpeedItemStartTime = (DateTime)dreader[46];
                            if(workItem.gameSpeedItemCnt > 0)
                            {
                                int minute = workItem.gameSpeedItemCnt * DWDataTableManager.GlobalSettingDataTable.GameSpeedItemTime;
                                gameSpeedItemStartTime = gameSpeedItemStartTime.AddMinutes(minute);
                                if(DateTime.UtcNow >= gameSpeedItemStartTime)
                                {
                                    workItem.gameSpeedItemCnt = 0;
                                    workItem.gameSpeedItemTimeRemain = 0;
                                }
                                else
                                {
                                    TimeSpan remainTime = gameSpeedItemStartTime - DateTime.UtcNow;
                                    workItem.gameSpeedItemTimeRemain = remainTime.Ticks;
                                }
                            }
                            else
                            {
                                workItem.gameSpeedItemTimeRemain = 0;
                            }
                            workItem.lastReturnStage = (long)dreader[47];
                            workItem.baseCampResetCnt = (long)dreader[48];
                            workItem.relicInventorySlotIdx = (byte)dreader[49];
                            workItem.droneAdvertisingOff = (bool)dreader[50];

                            result.userDataList.Add(workItem);
                            result.errorCode = (byte)DW_ERROR_CODE.OK;
                        }
                    }
                }
            }

            {
                result.userDataList[0].dailyQuestList.Clear();
                List<ulong> dailyQuestNoList = DWDataTableManager.GetDailyQuestList();
                for (int i = (int)DAILY_QUEST_GRADE_TYPE.GRADE_1; i < (int)DAILY_QUEST_GRADE_TYPE.MAX_TYPE; ++i)
                {
                    QuestData dailyQuestData = new QuestData();
                    dailyQuestData.serialNo = dailyQuestNoList[i - 1];
                    dailyQuestData.complete = 0;
                    dailyQuestData.getReward = 0;
                    dailyQuestData.curValue = "0";

                    result.userDataList[0].dailyQuestList.Add(dailyQuestData);
                }

                result.userDataList[0].achievementList.Clear();
                List<ulong> achievementNoList = DWDataTableManager.FirstAchievementList();
                for (int i = 0; i < achievementNoList.Count; ++i)
                {
                    QuestData achievementData = new QuestData();
                    achievementData.serialNo = achievementNoList[i];
                    achievementData.complete = 0;
                    achievementData.getReward = 0;
                    achievementData.curValue = "0";

                    result.userDataList[0].achievementList.Add(achievementData);
                }
            }

            // Init Unit
            // 유닛이 하나도 없다면 하나를 넣어준다.
            if (result.userDataList[0].unitList.Count == 0)
            {
                List<ulong> unitList = DWDataTableManager.GetFirstUnitList();
                for (int i = 0; i < unitList.Count; ++i)
                {
                    UnitData unitData = new UnitData();
                    unitData.serialNo = unitList[i];
                    unitData.level = 1;

                    result.userDataList[0].unitList.Add(unitData);
                }

                //result.userDataList[0].dailyQuestList.Clear();
                //List<ulong> dailyQuestNoList = DWDataTableManager.GetDailyQuestList();
                //for (int i = (int)DAILY_QUEST_GRADE_TYPE.GRADE_1; i < (int)DAILY_QUEST_GRADE_TYPE.MAX_TYPE; ++i)
                //{
                //    QuestData dailyQuestData = new QuestData();
                //    dailyQuestData.serialNo = dailyQuestNoList[i - 1];
                //    dailyQuestData.complete = 0;
                //    dailyQuestData.getReward = 0;
                //    dailyQuestData.curValue = "0";

                //    result.userDataList[0].dailyQuestList.Add(dailyQuestData);
                //}

                //result.userDataList[0].achievementList.Clear();
                //List<ulong> achievementNoList = DWDataTableManager.FirstAchievementList();
                //for (int i = 0; i < achievementNoList.Count; ++i)
                //{
                //    QuestData achievementData = new QuestData();
                //    achievementData.serialNo = achievementNoList[i];
                //    achievementData.complete = 0;
                //    achievementData.getReward = 0;
                //    achievementData.curValue = "0";

                //    result.userDataList[0].achievementList.Add(achievementData);
                //}

                result.userDataList[0].skillItemList.Clear();
                // Init Skill Item
                List<SkillItemDataTable> skillItemList = DWDataTableManager.GetFirstSkillItemList();
                for (int i = 0; i < skillItemList.Count; ++i)
                {
                    SkillItemData itemData = new SkillItemData();
                    itemData.type = skillItemList[i].Type;
                    itemData.count = 0;

                    result.userDataList[0].skillItemList.Add(itemData);
                }

                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("UPDATE DWMembersNew SET UnitList = @unitList, DailyQuestList = @dailyQuestList, AchievementList=@achievementList, SkillItemList = @skillItemList, DailyQuestAcceptTime = @dailyQuestAcceptTime WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].unitList);
                        command.Parameters.Add("@dailyQuestList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].dailyQuestList);
                        command.Parameters.Add("@achievementList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].achievementList);
                        command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].skillItemList);
                        command.Parameters.Add("@dailyQuestAcceptTime", SqlDbType.DateTime).Value = DateTime.UtcNow;  

                        connection.OpenWithRetry(retryPolicy);

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount <= 0)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }
                    }
                }
            }
            //---------------------------------------------------------

            //result.attendanceCheck = AttendanceCheckManager.AttendanceCheck(p.memberID, out result.userDataList[0].continueAttendanceCnt, out result.userDataList[0].continueAttendanceNo, out result.userDataList[0].accAttendanceCnt, out result.userDataList[0].accAttendanceNo) == true ? (byte)1 : (byte)0;

            int KOREA_TIME_ZONE = 9;
            DWMemberData.BossDungeonTicketRefresh(ref bossDungeonTicketRefreshTime, ref result.userDataList[0].bossDungeonTicket, KOREA_TIME_ZONE, DWDataTableManager.GlobalSettingDataTable.BossDugeonTicketCount);

            result.refreshDailyQeust = DWMemberData.DailyQuestRefresh(ref dailyQuestAcceptTime, ref result.userDataList[0].dailyQuestList, ref result.userDataList[0].dailyQeustTimeRemain) == true ? (byte)1 : (byte)0;
            if(result.userDataList[0].achievementList.Count == 0)
            {
                List<ulong> achievementNoList = DWDataTableManager.FirstAchievementList();
                for(int i = 0; i < achievementNoList.Count; ++i)
                {
                    QuestData achievementData = new QuestData();
                    achievementData.serialNo = achievementNoList[i];
                    achievementData.complete = 0;
                    achievementData.curValue = "0";
                    achievementData.getReward = 0;

                    result.userDataList[0].achievementList.Add(achievementData);
                }
            }

            DWMemberData.RefreshDrillIdx(resourceDrillStartTime, ref result.userDataList[0].resourceDrillIdx, ref result.userDataList[0].resourceDrillTimeRemain);
            DWMemberData.RefreshLuckySupplyShipRemainTime(luckySupplyShipLastTime, ref result.userDataList[0].luckySupplyShipTimeRemain);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET BossDungeonTicketRefreshTime = @bossDungeonTicketRefreshTime, BossDungeonTicket = @bossDungeonTicket, BossDungeonEnterType = @bossDungeonEnterType, DailyQuestList = @dailyQuestList, DailyQuestAcceptTime = @dailyQuestAcceptTime, AchievementList = @achievementList, ResouceDrillIdx = @resouceDrillIdx, DestroyScienceDrone = @destroyScienceDrone, GameSpeedItemCount = @gameSpeedItemCount WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@bossDungeonTicketRefreshTime", SqlDbType.DateTime).Value = bossDungeonTicketRefreshTime;
                    command.Parameters.Add("@bossDungeonTicket", SqlDbType.Int).Value = result.userDataList[0].bossDungeonTicket;
                    command.Parameters.Add("@bossDungeonEnterType", SqlDbType.TinyInt).Value = (byte)BOSS_DUNGEON_ENTER_TYPE.MAX_TYPE;
                    command.Parameters.Add("@dailyQuestList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].dailyQuestList);
                    command.Parameters.Add("@dailyQuestAcceptTime", SqlDbType.DateTime).Value = dailyQuestAcceptTime;
                    command.Parameters.Add("@achievementList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userDataList[0].achievementList);
                    command.Parameters.Add("@resouceDrillIdx", SqlDbType.TinyInt).Value = result.userDataList[0].resourceDrillIdx;
                    command.Parameters.Add("@destroyScienceDrone", SqlDbType.Bit).Value = 1;
                    command.Parameters.Add("@gameSpeedItemCount", SqlDbType.TinyInt).Value = result.userDataList[0].gameSpeedItemCnt;
                    
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
