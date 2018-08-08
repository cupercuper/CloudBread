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
    public class DWGetRewardDailyQuestController : ApiController
    {
        // GET api/DWGetRewardDailyQuest
        public string Get()
        {
            return "Hello from custom controller!";
        }


        public HttpResponseMessage Post(DWGetRewardDailyQuestInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWGetRewardDailyQuestInputParam>(decrypted);

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
                DWGetRewardDailyQuestModel result = result = GetResult(p);

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
                logMessage.Logger = "DWGetRewardDailyQuestController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWGetRewardDailyQuestModel GetResult(DWGetRewardDailyQuestInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWGetRewardDailyQuestModel result = new DWGetRewardDailyQuestModel();

            List<QuestData> dailyQuestList = null;
            double gold = 0;
            long gem = 0;
            long cashGem = 0;
            long ether = 0;
            long cashEther = 0;
            long gas = 0;
            long cashGas = 0;
            short lastWorld = 0;
            short lastStage = 0;
            List<SkillItemData> skillItemList = null;
            List<BoxData> boxList = null;
            long relicBoxCnt = 0;
            bool droneAdvertisingOff = false;


            // Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT DailyQuestList, Gem, CashGem, Ether, CashEther, Gas, CashGas, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage, DroneAdvertisingOff FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            logMessage.Logger = "DWGetRewardDailyQuestController";
                            logMessage.Message = string.Format("Select Failed");
                            Logging.RunLog(logMessage);

                            return result;
                        }

                        while (dreader.Read())
                        {
                            dailyQuestList = DWMemberData.ConvertQuestDataList(dreader[0] as byte[]);
                            gem = (long)dreader[1];
                            cashGem = (long)dreader[2];
                            ether = (long)dreader[3];
                            cashEther = (long)dreader[4];
                            gas = (long)dreader[5];
                            cashGas = (long)dreader[6];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[7] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[8] as byte[]);
                            relicBoxCnt = (long)dreader[9];
                            lastWorld = (short)dreader[10];
                            lastStage = (short)dreader[11];
                            droneAdvertisingOff = (bool)dreader[12];
                        }
                    }
                }
            }

            if (dailyQuestList.Count <= p.questIdx || dailyQuestList[p.questIdx].complete == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            DailyQuestDataTable dailyQuestDataTable = DWDataTableManager.GetDataTable(DailyQuestDataTable_List.NAME, dailyQuestList[p.questIdx].serialNo) as DailyQuestDataTable;
            if(dailyQuestDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            DWItemData itemData = new DWItemData();
            itemData.itemType = dailyQuestDataTable.ItemType;
            itemData.subType = dailyQuestDataTable.ItemSubType;
            itemData.value = dailyQuestDataTable.ItemValue;

            DWMemberData.AddItem(itemData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, ref droneAdvertisingOff, stageNo, logMessage);

            dailyQuestList[p.questIdx].getReward = 1;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET DailyQuestList = @dailyQuestList, Gem = @gem, Ether = @ether, Gas = @gas, SkillItemList = @skillItemList, BoxList = @boxList, DroneAdvertisingOff = @droneAdvertisingOff WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@dailyQuestList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(dailyQuestList);
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);
                    command.Parameters.Add("@droneAdvertisingOff", SqlDbType.Bit).Value = droneAdvertisingOff;
                    
                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWGetRewardDailyQuestController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.mineral = gold;
            result.gem = gem;
            result.ether = ether;
            result.gas = gas;
            result.relicBoxCnt = relicBoxCnt;
            result.skillItemList = skillItemList;
            result.boxList = boxList;
            result.droneAdvertisingOff = droneAdvertisingOff;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
