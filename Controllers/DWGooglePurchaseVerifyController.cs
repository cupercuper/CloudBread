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
    public class DWGooglePurchaseVerifyController : ApiController
    {
        // GET api/DWGooglePurchaseVerify
        public HttpResponseMessage Post(DWGooglePurchaseVerifyInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWGooglePurchaseVerifyInputParam>(decrypted);

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
                DWGooglePurchaseVerifyModel result = result = GetResult(p);

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

        DWGooglePurchaseVerifyModel GetResult(DWGooglePurchaseVerifyInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            DWGooglePurchaseVerifyModel result = new DWGooglePurchaseVerifyModel();

            result.unitDataList = new List<ClientUnitData>();
            result.activeItemList = new List<ActiveItemData>();
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

            for (int i = 0; i < p.purchasesList.Count; ++i)
            {
                DWGoogleGooglePurchaseVerifyData verifyData = p.purchasesList[i];
                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("SELECT [Index] FROM DWGooglePurchasesToken WHERE MemberID = '{0}', Token = '{1}'", p.memberID, verifyData.purchasesToken);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        connection.OpenWithRetry(retryPolicy);
                        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                        {
                            if (dreader.HasRows == true)
                            {
                                result.errorCode = (byte)DW_ERROR_CODE.PURCHAESE_ERROR_INTABLE;
                                logMessage.memberID = p.memberID;
                                logMessage.Level = "INFO";
                                logMessage.Logger = "DWGooglePurchaseVerifyController";
                                logMessage.Message = string.Format("DWGooglePurchasesToken in  verifyData.purchasesToken memberID = {0}, verifyData.purchasesToken = {1}", p.memberID, verifyData.purchasesToken);
                                Logging.RunLog(logMessage);
                                return result;
                            }
                        }
                    }
                }

                bool verify = GoogleJsonWebToken.instance.RequestVerifyFromGoogleStore(verifyData.productId, verifyData.purchasesToken, verifyData.packageName);
                if(verify == false)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.PURCHAESE_ERROR_VERIFY;
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWGooglePurchaseVerifyController";
                    logMessage.Message = string.Format("DWGooglePurchasesToken verify error Hack memberID = {0}, verifyData.purchasesToken = {1}", p.memberID, verifyData.purchasesToken);
                    Logging.RunLog(logMessage);

                    return result;
                }

                ShopDataTable shopDataTable = DWDataTableManager.GetShopTable(p.productId);
                if(shopDataTable == null)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWGooglePurchaseVerifyController";
                    logMessage.Message = string.Format("Not Fount ShopDataTable productId = {0}", p.productId);
                    Logging.RunLog(logMessage);

                    return result;
                }

                for(int k = 0; k < shopDataTable.ItemList.Count; ++k)
                {
                    ItemDataTable itemDataTable = DWDataTableManager.GetDataTable(ItemDataTable_List.NAME, shopDataTable.ItemList[k]) as ItemDataTable;
                    if(itemDataTable == null)
                    {
                        continue;
                    }

                    switch((ITEM_TYPE)itemDataTable.ChangeType)
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
                                if(unitSlotDataTable.UnitMaxCount == unitDic.Count)
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
                                    if(unitSummonRandomTicketDataTable == null)
                                    {
                                        return result;
                                    }

                                    ulong serialNo = DWDataTableManager.GetUnitTicket((DWDataTableManager.GROUP_ID)unitSummonRandomTicketDataTable.GroupID);
                                    if(serialNo == 0)
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
                                ActiveItemData activeItemData = new ActiveItemData();
                                activeItemData.itemType = (byte)ACTIVE_ITEM_TYPE.AUTO_GET_ITEM;
                                activeItemData.startTime = DateTime.UtcNow.Ticks;
                                int limitTime = int.Parse(itemDataTable.Value);
                                activeItemData.limitTime = limitTime == 0 ? -1 : limitTime;

                                result.activeItemList.Add(activeItemData);
                                activeItemList.Add(activeItemData);

                            }
                            break;
                        case ITEM_TYPE.SPEED_UP_2X_TYPE:
                            {
                                ActiveItemData activeItemData = new ActiveItemData();
                                activeItemData.itemType = (byte)ACTIVE_ITEM_TYPE.GAME_SPEED_UP_2X;
                                activeItemData.startTime = DateTime.UtcNow.Ticks;
                                int limitTime = int.Parse(itemDataTable.Value);
                                activeItemData.limitTime = limitTime == 0 ? -1 : limitTime;

                                result.activeItemList.Add(activeItemData);
                                activeItemList.Add(activeItemData);
                            }
                            break;
                        case ITEM_TYPE.UNIT_ATTACK_COOLTIME_TYPE:
                            {
                                ActiveItemData activeItemData = new ActiveItemData();
                                activeItemData.itemType = (byte)ACTIVE_ITEM_TYPE.UNIT_ATTACK_COOL_TIME;
                                activeItemData.startTime = DateTime.UtcNow.Ticks;
                                int limitTime = int.Parse(itemDataTable.Value);
                                activeItemData.limitTime = limitTime == 0 ? -1 : limitTime;

                                result.activeItemList.Add(activeItemData);
                                activeItemList.Add(activeItemData);
                            }
                            break;
                    }

                    using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                    {
                        string strQuery = "Insert into DWGooglePurchasesToken (MemberID, Token, ProductId) VALUES (@memberID, @token, @productId)";
                        using (SqlCommand command = new SqlCommand(strQuery, connection))
                        {
                            command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = p.memberID;
                            command.Parameters.Add("@token", SqlDbType.NVarChar).Value = verifyData.purchasesToken;
                            command.Parameters.Add("@productId", SqlDbType.NVarChar).Value = verifyData.productId;

                            connection.OpenWithRetry(retryPolicy);

                            int rowCount = command.ExecuteNonQuery();
                            if (rowCount <= 0)
                            {
                                logMessage.memberID = p.memberID;
                                logMessage.Level = "INFO";
                                logMessage.Logger = "DWGooglePurchaseVerifyController";
                                logMessage.Message = string.Format("Insert Failed DWGooglePurchasesToken");
                                Logging.RunLog(logMessage);

                                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                                return result;
                            }
                        }
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
            }

            result.gold = gold;
            result.gem = gem;
            result.cashGem = cashGem;
            result.enhancedStone = enhancedStone;
            result.cashEnhancedStone = cashEnhancedStone;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
