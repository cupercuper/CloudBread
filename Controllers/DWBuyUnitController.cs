﻿using System;
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
            int gem = 0;
            int enhancedStone = 0;
            byte unitSlotIdx = 1;
            byte unitStore = 0;
            List<UnitStoreData> unitStoreList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT UnitList, CanBuyUnitList, Gem, EnhancedStone, UnitSlotIdx, UnitStore, UnitStoreList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
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
                            gem = (int)dreader[2];
                            enhancedStone = (int)dreader[3];
                            unitSlotIdx = (byte)dreader[4];
                            unitStore = (byte)dreader[5];
                            unitStoreList = DWMemberData.ConvertUnitStoreList(dreader[6] as byte[]);
                        }
                    }
                }
            }

            if (unitList == null || canBuyUnitList == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
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
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("UnitSlotDataTable = null SerialNo = {0}", unitSlotIdx);
                Logging.RunLog(logMessage);
                return result;
            }

            if (unitSlotDataTable.UnitMaxCount == unitList.Count && unitStore == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("UnitSlotDataTable MaxCount SerialNo = {0}, Cur Unit Count = {1}", unitSlotIdx, unitList.Count);
                Logging.RunLog(logMessage);
                return result;
            }

            if (canBuyUnitList.Count == 0 || canBuyUnitList.Count <= p.index || p.index < 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("CanBuyUnitList Error Cur Index = {0}", p.index);
                Logging.RunLog(logMessage);
                return result;
            }

            if (p.unitStore == 1 && unitStore == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
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
                logMessage.Level = "INFO";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found UnitSummonDataTable SerialNo = {0}", serialNo);
                Logging.RunLog(logMessage);
                return result;
            }

            switch ((MONEY_TYPE)unitSummonDataTable.BuyType)
            {
                case MONEY_TYPE.ENHANCEDSTONE_TYPE:
                    if (enhancedStone < unitSummonDataTable.BuyCount)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWBuyUnitController";
                        logMessage.Message = string.Format("Lack EnhancedStone Cur EnhancedStone = {0}", enhancedStone);
                        Logging.RunLog(logMessage);

                        return result;
                    }
                    else
                    {
                        enhancedStone -= unitSummonDataTable.BuyCount;
                    }
                    break;
                case MONEY_TYPE.GEM_TYPE:
                    if (gem < unitSummonDataTable.BuyCount)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWBuyUnitController";
                        logMessage.Message = string.Format("Lack Gem Cur Gem = {0}", gem);
                        Logging.RunLog(logMessage);

                        return result;
                    }
                    else
                    {
                        gem -= unitSummonDataTable.BuyCount;
                    }
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
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWBuyUnitController";
                    logMessage.Message = string.Format("UnitList Error  InstanceNo = {0}", instanceNo);
                    Logging.RunLog(logMessage);

                    return result;
                }

                using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
                {
                    string strQuery = string.Format("UPDATE DWMembers SET UnitList = @unitList, CanBuyUnitList = @canBuyUnitList, Gem = @gem, EnhancedStone = @enhancedStone WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
                        command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(canBuyUnitList);
                        command.Parameters.Add("@gem", SqlDbType.Int).Value = gem;
                        command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = enhancedStone;

                        connection.OpenWithRetry(retryPolicy);

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount <= 0)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
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
                logMessage.Message = string.Format("UnitSummonNo = {0}, UnitSerialNo = {1}, CurCurEnhancementStone = {2}, CurGem = {3}", serialNo, unitData.SerialNo, enhancedStone, gem);
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
                    string strQuery = string.Format("UPDATE DWMembers SET UnitStoreList = @unitStoreList, CanBuyUnitList = @canBuyUnitList, Gem = @gem, EnhancedStone = @enhancedStone WHERE MemberID = '{0}'", p.memberID);
                    using (SqlCommand command = new SqlCommand(strQuery, connection))
                    {
                        command.Parameters.Add("@unitStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitStoreList);
                        command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(canBuyUnitList);
                        command.Parameters.Add("@gem", SqlDbType.Int).Value = gem;
                        command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = enhancedStone;

                        connection.OpenWithRetry(retryPolicy);

                        int rowCount = command.ExecuteNonQuery();
                        if (rowCount <= 0)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
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
                logMessage.Message = string.Format("Unit Store UnitSummonNo = {0}, UnitSerialNo = {1}, UnitCount = {2}, CurCurEnhancementStone = {3}, CurGem = {4}", serialNo, unitStoreData.serialNo, unitStoreData.count, enhancedStone, gem);
                Logging.RunLog(logMessage);
            }

            result.gem = gem;
            result.enhancedStone = enhancedStone;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;  

        }
    }
}
