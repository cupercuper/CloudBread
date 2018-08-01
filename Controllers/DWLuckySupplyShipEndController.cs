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
using StackExchange.Redis;
using CloudBreadRedis;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWLuckySupplyShipEndController : ApiController
    {
        // GET api/DWLuckySupplyShipEnd
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWLuckySupplyShipEndInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWLuckySupplyShipEndInputParam>(decrypted);

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
                DWLuckySupplyShipEndModel result = GetResult(p);

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
                logMessage.Logger = "DWLuckySupplyShipStartController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWLuckySupplyShipEndModel GetResult(DWLuckySupplyShipEndInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWLuckySupplyShipEndModel result = new DWLuckySupplyShipEndModel();

            double gold = 0;
            long gem = 0;
            long cashGem = 0;
            long ether = 0;
            long cashEther = 0;
            long gas = 0;
            long cashGas = 0;
            List<SkillItemData> skillItemList = null;
            List<BoxData> boxList = null;
            long relicBoxCnt = 0;
            short lastWorld = 0;
            short lastStage = 0;

            // Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, Ether, CashEther, Gas, CashGas, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage  FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            ether = (long)dreader[2];
                            cashEther = (long)dreader[3];
                            gas = (long)dreader[4];
                            cashGas = (long)dreader[5];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[6] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[7] as byte[]);
                            relicBoxCnt = (long)dreader[8];
                            lastWorld = (short)dreader[9];
                            lastStage = (short)dreader[10];
                        }
                    }
                }
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            byte[] value = CBRedis.GetRedisKeyValue(RedisIndex.LUCKY_SUPPLY_SHIP_RANK_IDX, p.memberID);
            LuckySupplyShipData shipData = DWMemberData.ConvertLuckySupplyShipData(value);

            CBRedis.KeyDelete(RedisIndex.LUCKY_SUPPLY_SHIP_RANK_IDX, p.memberID);
            //아이템을 넣어 준다.
            for (int i = 0; i < shipData.itemList.Count; ++i)
            {
                DWMemberData.AddItem(shipData.itemList[i], ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, stageNo, logMessage, false);   
            }

            DateTime lastTime = DateTime.UtcNow.AddHours(DWDataTableManager.GlobalSettingDataTable.LuckySupplyShipResetTime);
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET LuckySupplyShipLastTime = @luckySupplyShipLastTime, Gem = @gem, Ether = @ether, Gas = @gas, SkillItemList = @skillItemList, BoxList = @boxList, RelicBoxCount = @relicBoxCount WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@luckySupplyShipLastTime", SqlDbType.DateTime).Value = lastTime;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);
                    command.Parameters.Add("@relicBoxCount", SqlDbType.BigInt).Value = relicBoxCnt;
                    
                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWBuyUnitController";
                        logMessage.Message = string.Format("Unit List Update Failed");
                        Logging.RunLog(logMessage);

                        return result;
                    }
                }
            }

            result.gold = gold;
            result.gem = gem;
            result.ether = ether;
            result.gas = gas;
            result.skillItemList = skillItemList;
            result.boxList = boxList;
            result.relicBoxCnt = relicBoxCnt;
            result.itemList = shipData.itemList;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
