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
using CloudBread.Manager;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWInsertUserDataController : ApiController
    {
        // GET api/DWInsertUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWInsUserDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWInsUserDataInputParams>(decrypted);

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
                DWInsUserDataModel result = GetResult(p);

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
                logMessage.Logger = "DWInsertUserDataController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWInsUserDataModel GetResult(DWInsUserDataInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWInsUserDataModel result = new DWInsUserDataModel();

            GlobalSettingDataTable globalSettingDataTable = DWDataTableManager.GetDataTable(GlobalSettingDataTable_List.NAME, 1) as GlobalSettingDataTable;
            if(globalSettingDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWInsertUserDataController";
                logMessage.Message = string.Format("not found GlobalSettingDataTable");
                Logging.RunLog(logMessage);
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            TimeSpan dailyQeustTimeRemain = new TimeSpan(globalSettingDataTable.DailyQuestResetTIme, 0, 0);

            result.userData = new DWUserData()
            {
                memberID = p.memberID,
                nickName = p.nickName,
                recommenderID = p.recommenderID,
                captianLevel = 0,
                captianID = 0,
                captianChange = 0,
                lastWorld = 1,
                curWorld = 1,
                curStage = 1,
                lastStage = 1,
                unitList = new List<UnitData>(),
                gold = 0,
                gem = 10000,
                cashGem = 0,
                ether = 0,
                cashEther = 0,
                gas = 0,
                cashGas = 0,
                activeItemList = new List<ActiveItemData>(),
                limitShopItemDataList = new List<LimitShopItemData>(),
                accStage = 1,
                bossDungeonTicket = globalSettingDataTable.BossDugeonTicketCount,
                curBossDungeonNo = 1,
                lastBossDungeonNo = 1,
                bossClearList = new List<uint>(),
                timeZoneID = p.timeZoneID,
                timeZoneTotalMin = p.timeZoneTotalMin,
                continueAttendanceCnt = 0,
                accAttendanceCnt = 0,
                continueAttendanceNo = 0,
                accAttendanceNo = 0,
                dailyQuestList = new List<QuestData>(),
                dailyQeustTimeRemain = dailyQeustTimeRemain.Ticks,
                achievementList = new List<QuestData>(),
                resourceDrillIdx = 0,
                resourceDrillTimeRemain = 0,
                luckySupplyShipTimeRemain = 0,
                skillItemList = new List<SkillItemData>(),
                boxList = new List<BoxData>(),
                relicList = new List<RelicData>(),
                relicStoreList = new List<RelicData>(),
                relicSlotIdx = 1,
                baseCampList = new List<BaseCampData>(),
                relicBoxCnt = 0,
                gameSpeedItemCnt = 0,
                gameSpeedItemTimeRemain = 0,
                lastReturnStage = 0,
                baseCampResetCnt = 0,
                relicInventorySlotIdx = 1,
            };

            List<ulong> dailyQuestNoList = DWDataTableManager.GetDailyQuestList();
            for (int i = (int)DAILY_QUEST_GRADE_TYPE.GRADE_1; i < (int)DAILY_QUEST_GRADE_TYPE.MAX_TYPE; ++i)
            {
                QuestData dailyQuestData = new QuestData();
                dailyQuestData.serialNo = dailyQuestNoList[i - 1];
                dailyQuestData.complete = 0;
                dailyQuestData.getReward = 0;
                dailyQuestData.curValue = "0";

                result.userData.dailyQuestList.Add(dailyQuestData);
            }

            List<ulong> achievementNoList = DWDataTableManager.FirstAchievementList();
            for(int i = 0; i < achievementNoList.Count; ++i)
            {
                QuestData achievementData = new QuestData();
                achievementData.serialNo = achievementNoList[i];
                achievementData.complete = 0;
                achievementData.getReward = 0;
                achievementData.curValue = "0";

                result.userData.achievementList.Add(achievementData);
            }

            //AttendanceCheckManager.AttendanceCheckInit(p.memberID, result.userData.timeZoneTotalMin, out result.userData.continueAttendanceNo, out result.userData.accAttendanceNo);

            // Init Unit
            List<ulong> unitList = DWDataTableManager.GetFirstUnitList();
            for (int i = 0; i < unitList.Count; ++i)
            {
                UnitData unitData = new UnitData();
                unitData.serialNo = unitList[i];
                unitData.level = 1;

                result.userData.unitList.Add(unitData);
            }
            //---------------------------------------------------------

            // Init Skill Item
            List<SkillItemDataTable> skillItemList = DWDataTableManager.GetFirstSkillItemList();
            for(int i = 0; i < skillItemList.Count; ++i)
            {
                SkillItemData itemData = new SkillItemData();
                itemData.type = skillItemList[i].Type;
                itemData.count = 0;

                result.userData.skillItemList.Add(itemData);
            }

            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "Insert into DWMembersNew (MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, CaptianChange, LastWorld, CurWorld, CurStage, LastStage, UnitList, Gold, Gem, CashGem, Ether, CashEther, ActiveItemList, LimitShopItemDataList, BossDungeonTicket, LastBossDungeonNo, BossClearList, TimeZone, TimeZoneID, ContinueAttendanceCnt, ContinueAttendanceNo, AccAttendanceCnt, AccAttendanceNo, DailyQuestList, AchievementList, ResouceDrillIdx, LuckySupplyShipLastTime, SkillItemList, BoxList, RelicList, RelicStoreList, RelicSlotIdx, Gas, CashGas, BaseCampList, RelicBoxCount, LastReturnStage, BaseCampResetCount, RelicInventorySlotIdx) VALUES (@memberID, @nickName, @recommenderID, @captianLevel, @captianID, @captianChange, @lastWorld, @curWorld, @curStage, @lastStage, @unitList, @gold, @gem, @cashGem, @ether, @cashEther, @activeItemList, @limitShopItemDataList, @bossDungeonTicket, @lastBossDungeonNo, @bossClearList, @timeZone, @timeZoneID, @continueAttendanceCnt, @continueAttendanceNo, @accAttendanceCnt, @accAttendanceNo, @dailyQuestList, @achievementList, @resouceDrillIdx, @luckySupplyShipLastTime, @skillItemList, @boxList, @relicList, @relicStoreList, @relicSlotIdx, @gas, @cashGas, @baseCampList, @relicBoxCount, @lastReturnStage, @baseCampResetCount, @relicInventorySlotIdx)";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = result.userData.memberID;
                    command.Parameters.Add("@nickName", SqlDbType.NVarChar).Value = result.userData.nickName;
                    command.Parameters.Add("@recommenderID", SqlDbType.NVarChar).Value = result.userData.recommenderID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = result.userData.captianLevel;
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = result.userData.captianID;
                    command.Parameters.Add("@captianChange", SqlDbType.BigInt).Value = result.userData.captianChange;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = result.userData.lastWorld;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = result.userData.curWorld;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = result.userData.curStage;
                    command.Parameters.Add("@lastStage", SqlDbType.SmallInt).Value = result.userData.curStage;
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.unitList);
                    command.Parameters.Add("@gold", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.gold);
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = result.userData.gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = result.userData.cashGem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = result.userData.ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = result.userData.cashEther;     
                    command.Parameters.Add("@activeItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.activeItemList);
                    command.Parameters.Add("@limitShopItemDataList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.limitShopItemDataList);
                    command.Parameters.Add("@bossDungeonTicket", SqlDbType.Int).Value = result.userData.bossDungeonTicket;
                    command.Parameters.Add("@lastBossDungeonNo", SqlDbType.SmallInt).Value = result.userData.lastBossDungeonNo;
                    command.Parameters.Add("@bossClearList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.bossClearList);
                    command.Parameters.Add("@timeZone", SqlDbType.Int).Value = result.userData.timeZoneTotalMin;
                    command.Parameters.Add("@timeZoneID", SqlDbType.NVarChar).Value = result.userData.timeZoneID;
                    command.Parameters.Add("@continueAttendanceCnt", SqlDbType.TinyInt).Value = result.userData.continueAttendanceCnt;
                    command.Parameters.Add("@continueAttendanceNo", SqlDbType.SmallInt).Value = result.userData.continueAttendanceNo;
                    command.Parameters.Add("@accAttendanceCnt", SqlDbType.TinyInt).Value = result.userData.accAttendanceCnt;
                    command.Parameters.Add("@accAttendanceNo", SqlDbType.SmallInt).Value = result.userData.accAttendanceNo;
                    command.Parameters.Add("@dailyQuestList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.dailyQuestList);
                    command.Parameters.Add("@achievementList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.achievementList);
                    command.Parameters.Add("@resouceDrillIdx", SqlDbType.TinyInt).Value = result.userData.resourceDrillIdx;
                    command.Parameters.Add("@luckySupplyShipLastTime", SqlDbType.DateTime).Value = new DateTime(1970,1,1);
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.boxList);
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(new Dictionary<uint, RelicData>());
                    command.Parameters.Add("@relicStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(new Dictionary<uint, RelicData>());
                    command.Parameters.Add("@relicSlotIdx", SqlDbType.TinyInt).Value = result.userData.relicSlotIdx;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = result.userData.gas;
                    command.Parameters.Add("@cashGas", SqlDbType.BigInt).Value = result.userData.cashGas;
                    command.Parameters.Add("@baseCampList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(new Dictionary<ulong, ushort>());
                    command.Parameters.Add("@relicBoxCount", SqlDbType.BigInt).Value = result.userData.relicBoxCnt;
                    command.Parameters.Add("@lastReturnStage", SqlDbType.BigInt).Value = result.userData.lastReturnStage;
                    command.Parameters.Add("@baseCampResetCount", SqlDbType.BigInt).Value = result.userData.baseCampResetCnt;
                    command.Parameters.Add("@relicInventorySlotIdx", SqlDbType.BigInt).Value = result.userData.relicInventorySlotIdx;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWInsertUserDataController";
                        logMessage.Message = string.Format("Insert Failed DWMembersNew");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            List<long> eventLIst = new List<long>();
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "Insert into DWMembersInputEventNew (MemberID, EventList) VALUES (@memberID, @eventList)";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = result.userData.memberID;
                    command.Parameters.Add("@eventList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(eventLIst);  

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWInsertUserDataController";
                        logMessage.Message = string.Format("Insert Failed DWMembersInputEventNew");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            //if (DWMemberData.IsTestMemberID(p.memberID) == false)
            //{
            //    CBRedis.SetSortedSetRank((int)RANK_TYPE.CUR_STAGE_TYPE, p.memberID, 1);
            //    CBRedis.SetSortedSetRank((int)RANK_TYPE.ACC_STAGE_TYPE, p.memberID, 1);
            //}

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWInsertUserDataController";
            logMessage.Message = string.Format("Insert User");
            Logging.RunLog(logMessage);

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
