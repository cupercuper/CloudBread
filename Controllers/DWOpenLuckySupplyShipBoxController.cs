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
    public class DWOpenLuckySupplyShipBoxController : ApiController
    {
        // GET api/DWOpenLuckySupplyShipBox
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWOpenLuckySupplyShipBoxInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWOpenLuckySupplyShipBoxInputParam>(decrypted);

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
                DWOpenLuckySupplyShipBoxModel result = GetResult(p);

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

        DWOpenLuckySupplyShipBoxModel GetResult(DWOpenLuckySupplyShipBoxInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWOpenLuckySupplyShipBoxModel result = new DWOpenLuckySupplyShipBoxModel();

            byte [] value = CBRedis.GetRedisKeyValue(RedisIndex.LUCKY_SUPPLY_SHIP_RANK_IDX, p.memberID);
            LuckySupplyShipData shipData = DWMemberData.ConvertLuckySupplyShipData(value);

            if(shipData.shipIdx + 1 != p.shipIdx)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            LuckySupplyShipDataTable luckySupplyShipDataTable = DWDataTableManager.GetDataTable(LuckySupplyShipDataTable_List.NAME, p.shipIdx) as LuckySupplyShipDataTable;
            if(luckySupplyShipDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            byte itemIdx = DWDataTableManager.GetLuckySupplyShipItemIdx(p.shipIdx);

            DWItemData itemData = new DWItemData();
            shipData.fail = luckySupplyShipDataTable.ItemTypeList[itemIdx] == -1 ? (byte)1 : (byte)0;
            if (shipData.fail != 1)
            {
                itemData.itemType = (byte)luckySupplyShipDataTable.ItemTypeList[itemIdx];
                itemData.subType = luckySupplyShipDataTable.ItemSubTypeList[itemIdx];
                itemData.value = luckySupplyShipDataTable.ItemValueLIst[itemIdx];

                double gold = 0;
                long gem = 0;
                long cashGem = 0;
                long ether = 0;
                long cashEther = 0;
                long gas = 0;
                long cashGas = 0;
                long relicBoxCnt = 0;
                List<SkillItemData> skillItemList = null;
                List<BoxData> boxList = null;
                short lastWorld = 0;
                short lastStage = 0;
                bool droneAdvertisingOff = false;

                RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("SELECT SkillItemList, LastWorld, LastStage FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        connection.OpenWithRetry(retryPolicy);
                        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                        {
                            if (dreader.HasRows == false)
                            {
                                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                                return result;
                            }

                            while (dreader.Read())
                            {
                                skillItemList = DWMemberData.ConvertSkillItemList(dreader[0] as byte[]);
                                lastWorld = (short)dreader[1];
                                lastStage = (short)dreader[2];
                            }
                        }
                    }
                }

                ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;
                DWMemberData.AddItem(itemData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, ref droneAdvertisingOff, stageNo, logMessage);
                shipData.itemList.Add(itemData);
            }

            shipData.shipIdx = p.shipIdx;

            CBRedis.SetRedisKey(RedisIndex.LUCKY_SUPPLY_SHIP_RANK_IDX, p.memberID, DWMemberData.ConvertByte(shipData), null);

            result.itemIdx = itemIdx;
            result.itemData = itemData;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
