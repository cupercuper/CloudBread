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
    public class DWShopController : ApiController
    {
        // GET api/DWShop
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWShopInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWShopInputParam>(decrypted);

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
                DWShopModel result = result = GetResult(p);

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
                logMessage.Logger = "DWGooglePurchaseVerifyController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWShopModel GetResult(DWShopInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            DWShopModel result = new DWShopModel();

            result.unitDataList = new List<ClientUnitData>();
            result.unitTicketList = new List<DWUnitTicketData>();

            long gold = 0;
            long gem = 0;
            long cashGem = 0;
            long enhancedStone = 0;
            long cashEnhancedStone = 0;
            byte unitSlotIdx = 0;
            Dictionary<uint, UnitData> unitDic = null;
            List<DWUnitTicketData> unitTicketList = null;
            List<ActiveItemData> activeItemList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gold, Gem, CashGem, EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitList, UnitTicketList, ActiveItemList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
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
                            gold = (long)dreader[0];
                            gem = (long)dreader[1];
                            cashGem = (long)dreader[2];
                            enhancedStone = (long)dreader[3];
                            cashEnhancedStone = (long)dreader[4];
                            unitSlotIdx = (byte)dreader[5];
                            unitDic = DWMemberData.ConvertUnitDic(dreader[6] as byte[]);
                            unitTicketList = DWMemberData.ConvertUnitTicketDataList(dreader[7] as byte[]);
                            activeItemList = DWMemberData.ConvertActiveItemList(dreader[8] as byte[]);
                        }
                    }
                }
            }

            DWMemberData.UpdateActiveItem(activeItemList);

            UnitSlotDataTable unitSlotDataTable = DWDataTableManager.GetDataTable(UnitSlotDataTable_List.NAME, unitSlotIdx) as UnitSlotDataTable;
            if (unitSlotDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWGooglePurchaseVerifyController";
                logMessage.Message = string.Format("UnitSlotDataTable = null SerialNo = {0}", unitSlotIdx);
                Logging.RunLog(logMessage);
                return result;
            }

            ShopDataTable shopDataTable = DWDataTableManager.GetDataTable(ShopDataTable_List.NAME, p.serialNo) as ShopDataTable;
            if (shopDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWGooglePurchaseVerifyController";
                logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                Logging.RunLog(logMessage);

                return result;
            }

            if((MONEY_TYPE)shopDataTable.MoneyType == MONEY_TYPE.CASH_TYPE)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWGooglePurchaseVerifyController";
                logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                Logging.RunLog(logMessage);

                return result;
            }

            switch((MONEY_TYPE)shopDataTable.MoneyType)
            {
                case MONEY_TYPE.GOLD_TYPE:
                    if(gold < shopDataTable.MoneyCount)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWGooglePurchaseVerifyController";
                        logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                        Logging.RunLog(logMessage);
                    }

                    gold -= shopDataTable.MoneyCount;
                    break;
                case MONEY_TYPE.GEM_TYPE:
                    if (DWMemberData.SubGem(ref gem, ref cashGem, shopDataTable.MoneyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWGooglePurchaseVerifyController";
                        logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                        Logging.RunLog(logMessage);
                    }
                    break;
                case MONEY_TYPE.ENHANCEDSTONE_TYPE:
                    if (DWMemberData.SubEnhancedStone(ref enhancedStone, ref cashEnhancedStone, shopDataTable.MoneyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWGooglePurchaseVerifyController";
                        logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                        Logging.RunLog(logMessage);
                    }
                    break;
            }

            for (int k = 0; k < shopDataTable.ItemList.Count; ++k)
            {
                ItemDataTable itemDataTable = DWDataTableManager.GetDataTable(ItemDataTable_List.NAME, shopDataTable.ItemList[k]) as ItemDataTable;
                if (itemDataTable == null)
                {
                    continue;
                }

                switch ((ITEM_TYPE)itemDataTable.ChangeType)
                {
                    case ITEM_TYPE.GOLD_TYPE:
                        {
                            gold += long.Parse(itemDataTable.Value);
                        }
                        break;
                    case ITEM_TYPE.GEM_TYPE:
                        {
                            DWMemberData.AddGem(ref gem, ref cashGem, 0, long.Parse(itemDataTable.Value), logMessage);
                        }
                        break;
                    case ITEM_TYPE.ENHANCEDSTONE_TYPE:
                        {
                            DWMemberData.AddEnhancedStone(ref enhancedStone, ref cashEnhancedStone, 0, long.Parse(itemDataTable.Value), logMessage);
                        }
                        break;
                    case ITEM_TYPE.UNIT_TYPE:
                        {
                            if (unitSlotDataTable.UnitMaxCount == unitDic.Count)
                            {
                                DWUnitTicketData ticketData = new DWUnitTicketData();
                                ticketData.ticketType = UNIT_SUMMON_TICKET_TYPE.FIX_TYPE;
                                ticketData.serialNo = ulong.Parse(itemDataTable.Value);

                                unitTicketList.Add(ticketData);
                                result.unitTicketList.Add(ticketData);
                            }
                            else
                            {
                                uint instanceNo = 0;
                                UnitData unitData = null;
                                instanceNo = DWMemberData.AddUnitDic(ref unitDic, ulong.Parse(itemDataTable.Value));
                                if (unitDic.TryGetValue(instanceNo, out unitData) == false)
                                {
                                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                                    logMessage.memberID = p.memberID;
                                    logMessage.Level = "INFO";
                                    logMessage.Logger = "DWGooglePurchaseVerifyController";
                                    logMessage.Message = string.Format("UnitList Error  InstanceNo = {0}", instanceNo);
                                    Logging.RunLog(logMessage);

                                    return result;
                                }

                                ClientUnitData clientUnitData = new ClientUnitData()
                                {
                                    instanceNo = instanceNo,
                                    level = unitData.Level,
                                    enhancementCount = unitData.EnhancementCount,
                                    serialNo = unitData.SerialNo
                                };

                                result.unitDataList.Add(clientUnitData);
                            }
                        }
                        break;
                    case ITEM_TYPE.UNIT_RANDOM_TYPE:
                        {
                            if (unitSlotDataTable.UnitMaxCount == unitDic.Count)
                            {
                                DWUnitTicketData ticketData = new DWUnitTicketData();
                                ticketData.ticketType = UNIT_SUMMON_TICKET_TYPE.RANDOM_TYPE;
                                ticketData.serialNo = ulong.Parse(itemDataTable.Value);

                                unitTicketList.Add(ticketData);

                                result.unitTicketList.Add(ticketData);
                            }
                            else
                            {
                                UnitSummonRandomTicketDataTable unitSummonRandomTicketDataTable = DWDataTableManager.GetDataTable(UnitSummonRandomTicketDataTable_List.NAME, ulong.Parse(itemDataTable.Value)) as UnitSummonRandomTicketDataTable;
                                if (unitSummonRandomTicketDataTable == null)
                                {
                                    return result;
                                }

                                ulong serialNo = DWDataTableManager.GetUnitTicket((DWDataTableManager.GROUP_ID)unitSummonRandomTicketDataTable.GroupID);
                                if (serialNo == 0)
                                {
                                    return result;
                                }

                                UnitSummonDataTable unitSummonDataTable = DWDataTableManager.GetDataTable(UnitSummonDataTable_List.NAME, serialNo) as UnitSummonDataTable;
                                if (unitSummonDataTable == null)
                                {
                                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                                    logMessage.memberID = p.memberID;
                                    logMessage.Level = "INFO";
                                    logMessage.Logger = "DWGooglePurchaseVerifyController";
                                    logMessage.Message = string.Format("Not Found UnitSummonDataTable SerialNo = {0}", serialNo);
                                    Logging.RunLog(logMessage);
                                    return result;
                                }

                                uint instanceNo = 0;
                                UnitData unitData = null;
                                instanceNo = DWMemberData.AddUnitDic(ref unitDic, unitSummonDataTable.ChangeSerialNo);
                                if (unitDic.TryGetValue(instanceNo, out unitData) == false)
                                {
                                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                                    logMessage.memberID = p.memberID;
                                    logMessage.Level = "INFO";
                                    logMessage.Logger = "DWGooglePurchaseVerifyController";
                                    logMessage.Message = string.Format("UnitList Error  InstanceNo = {0}", instanceNo);
                                    Logging.RunLog(logMessage);

                                    return result;
                                }

                                ClientUnitData clientUnitData = new ClientUnitData()
                                {
                                    instanceNo = instanceNo,
                                    level = unitData.Level,
                                    enhancementCount = unitData.EnhancementCount,
                                    serialNo = unitData.SerialNo
                                };

                                result.unitDataList.Add(clientUnitData);
                            }
                        }
                        break;
                    case ITEM_TYPE.AUTO_GET_ITEM_TYPE:
                        {
                            DWMemberData.AddActiveItem(activeItemList, ACTIVE_ITEM_TYPE.AUTO_GET_ITEM, int.Parse(itemDataTable.Value));
                        }
                        break;
                    case ITEM_TYPE.SPEED_UP_2X_TYPE:
                        {
                            DWMemberData.AddActiveItem(activeItemList, ACTIVE_ITEM_TYPE.GAME_SPEED_UP_2X, int.Parse(itemDataTable.Value));
                        }
                        break;
                    case ITEM_TYPE.UNIT_ATTACK_COOLTIME_TYPE:
                        {
                            DWMemberData.AddActiveItem(activeItemList, ACTIVE_ITEM_TYPE.UNIT_ATTACK_COOL_TIME, int.Parse(itemDataTable.Value));
                        }
                        break;
                }

                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("UPDATE DWMembers SET Gold = @gold, Gem = @gem, CashGem = @cashGem, EnhancedStone = @enhancedStone, CashEnhancedStone = @cashEnhancedStone, UnitList = @unitList, UnitTicketList = @unitTicketList, ActiveItemList = @activeItemList WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@gold", SqlDbType.BigInt).Value = gold;
                        command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                        command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                        command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                        command.Parameters.Add("@cashEnhancedStone", SqlDbType.BigInt).Value = cashEnhancedStone;
                        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitDic);
                        command.Parameters.Add("@unitTicketList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitTicketList);
                        command.Parameters.Add("@activeItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(activeItemList);

                        connection.OpenWithRetry(retryPolicy);

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount <= 0)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWGooglePurchaseVerifyController";
                            logMessage.Message = string.Format("DWMembers Udpate Failed");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }
                    }
                } 
            }

            result.gold = gold;
            result.gem = gem;
            result.cashGem = cashGem;
            result.enhancedStone = enhancedStone;
            result.cashEnhancedStone = cashEnhancedStone;
            result.activeItemList = activeItemList;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
