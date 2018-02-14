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
    public class DWBuyUnitController : ApiController
    {
        // GET api/DWBuyUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBuyUnitInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBuyUnitInputParam>(decrypted);

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
                DWBuyUnitModel result = GetResult(p);

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
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWBuyUnitModel GetResult(DWBuyUnitInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWBuyUnitModel result = new DWBuyUnitModel();

            Dictionary<uint, UnitData> unitList = null;
            List<ulong> canBuyUnitList = null;
            long gem = 0;
            long enhancedStone = 0;
            long cashGem = 0;
            long cashEnhancedStone = 0;

            byte unitSlotIdx = 1;
            byte unitStore = 0;
            List<UnitStoreData> unitStoreList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT UnitList, CanBuyUnitList, Gem, CashGem, EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitStore, UnitStoreList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWBuyUnitController";
                            logMessage.Message = string.Format("Not Found User = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            unitList = DWMemberData.ConvertUnitDic(dreader[0] as byte[]);
                            canBuyUnitList = DWMemberData.ConvertUnitList(dreader[1] as byte[]);
                            gem = (long)dreader[2];
                            cashGem = (long)dreader[3];
                            enhancedStone = (long)dreader[4];
                            cashEnhancedStone = (long)dreader[5];
                            unitSlotIdx = (byte)dreader[6];
                            unitStore = (byte)dreader[7];
                            unitStoreList = DWMemberData.ConvertUnitStoreList(dreader[8] as byte[]);
                        }
                    }
                }
            }

            if (unitList == null || canBuyUnitList == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found unitList OR canBuyUnitList = {0}", p.memberID);
                Logging.RunLog(logMessage);

                return result;
            }

            UnitSlotDataTable unitSlotDataTable = DWDataTableManager.GetDataTable(UnitSlotDataTable_List.NAME, unitSlotIdx) as UnitSlotDataTable;
            if (unitSlotDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("UnitSlotDataTable = null SerialNo = {0}", unitSlotIdx);
                Logging.RunLog(logMessage);
                return result;
            }

            if (unitSlotDataTable.UnitMaxCount == unitList.Count && unitStore == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("UnitSlotDataTable MaxCount SerialNo = {0}, Cur Unit Count = {1}", unitSlotIdx, unitList.Count);
                Logging.RunLog(logMessage);
                return result;
            }

            if (canBuyUnitList.Count == 0 || canBuyUnitList.Count <= p.index || p.index < 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("CanBuyUnitList Error Cur Index = {0}", p.index);
                Logging.RunLog(logMessage);
                return result;
            }

            if (p.unitStore == 1 && unitStore == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Open Unit Store", p.index);
                Logging.RunLog(logMessage);
                return result;
            }

            ulong serialNo = canBuyUnitList[p.index];
            UnitSummonDataTable unitSummonDataTable = DWDataTableManager.GetDataTable(UnitSummonDataTable_List.NAME, serialNo) as UnitSummonDataTable;
            if (unitSummonDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found UnitSummonDataTable SerialNo = {0}", serialNo);
                Logging.RunLog(logMessage);
                return result;
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWBuyUnitController";

            switch ((MONEY_TYPE)unitSummonDataTable.BuyType)
            {
                case MONEY_TYPE.ENHANCEDSTONE_TYPE:
                    
                    if (DWMemberData.SubEnhancedStone(ref enhancedStone, ref cashEnhancedStone, unitSummonDataTable.BuyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                        logMessage.Level = "Error";
                        logMessage.Message = string.Format("Lack EnhancedStone Cur EnhancedStone = {0}", enhancedStone);
                        Logging.RunLog(logMessage);

                        return result;
                    }
                    Logging.RunLog(logMessage);

                    break;
                case MONEY_TYPE.GEM_TYPE:

                    if (DWMemberData.SubGem(ref gem, ref cashGem, unitSummonDataTable.BuyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                        logMessage.Level = "Error";
                        logMessage.Message = string.Format("Lack Gem Cur Gem = {0}", gem);
                        Logging.RunLog(logMessage);

                        return result;
                    }
                    Logging.RunLog(logMessage);
                    break;
            }

            canBuyUnitList.RemoveAt(p.index);

            result.unitDataList = new List<ClientUnitData>();
            result.unitStoreDataList = new List<UnitStoreData>();

            if (p.unitStore == 0)
            {
                uint instanceNo = 0;
                UnitData unitData = null;
                instanceNo = DWMemberData.AddUnitDic(ref unitList, unitSummonDataTable.ChangeSerialNo);
                if (unitList.TryGetValue(instanceNo, out unitData) == false)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWBuyUnitController";
                    logMessage.Message = string.Format("UnitList Error  InstanceNo = {0}", instanceNo);
                    Logging.RunLog(logMessage);

                    return result;
                }

                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("UPDATE DWMembers SET UnitList = @unitList, CanBuyUnitList = @canBuyUnitList, Gem = @gem, CashGem = @cashGem, EnhancedStone = @enhancedStone, CashEnhancedStone = @cashEnhancedStone WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
                        command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(canBuyUnitList);
                        command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                        command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                        command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                        command.Parameters.Add("@cashEnhancedStone", SqlDbType.BigInt).Value = cashEnhancedStone;

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

                ClientUnitData clientUnitData = new ClientUnitData()
                {
                    instanceNo = instanceNo,
                    level = unitData.Level,
                    enhancementCount = unitData.EnhancementCount,
                    serialNo = unitData.SerialNo
                };

                result.unitDataList.Add(clientUnitData);

                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("UnitSummonNo = {0}, UnitSerialNo = {1}, CurEnhancementStone = {2}, CurCashEnhancementStone = {3}, CurGem = {4}, CurCashGem = {5}", serialNo, unitData.SerialNo, enhancedStone, cashEnhancedStone, gem, cashGem);
                Logging.RunLog(logMessage);

            }
            else
            {
                UnitStoreData unitStoreData = null;
                unitStoreData = unitStoreList.Find(x => x.serialNo == unitSummonDataTable.ChangeSerialNo);
                if (unitStoreData == null)
                {
                    unitStoreData = new UnitStoreData()
                    {
                        serialNo = unitSummonDataTable.ChangeSerialNo,
                        count = 1
                    };

                    unitStoreList.Add(unitStoreData);
                }
                else
                {
                    unitStoreData.count++;
                }

                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("UPDATE DWMembers SET UnitStoreList = @unitStoreList, CanBuyUnitList = @canBuyUnitList, Gem = @gem, CashGem = @cashGem, EnhancedStone = @enhancedStone, CashEnhancedStone = @cashEnhancedStone WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@unitStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitStoreList);
                        command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(canBuyUnitList);
                        command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                        command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                        command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                        command.Parameters.Add("@cashEnhancedStone", SqlDbType.BigInt).Value = cashEnhancedStone;

                        connection.OpenWithRetry(retryPolicy);

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount <= 0)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWBuyUnitController";
                            logMessage.Message = string.Format("Unit Store Update Failed");
                            Logging.RunLog(logMessage);

                            return result;
                        }
                    }
                }

                result.unitStoreDataList.Add(unitStoreData);

                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Unit Store UnitSummonNo = {0}, UnitSerialNo = {1}, UnitCount = {2}, CurEnhancementStone = {3}, CurCashEnhancementStone = {4}, CurGem = {5}, CurCashGem = {6}", serialNo, unitStoreData.serialNo, unitStoreData.count, enhancedStone, cashEnhancedStone, gem, cashGem);
                Logging.RunLog(logMessage);
            }

            result.index = p.index;
            result.gem = gem;
            result.cashGem = cashGem;
            result.enhancedStone = enhancedStone;
            result.cashEnhancedStone = cashEnhancedStone;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;  

        }
    }
}
