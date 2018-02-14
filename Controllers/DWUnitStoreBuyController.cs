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
    public class DWUnitStoreBuyController : ApiController
    {
        // GET api/DWUnitStoreBuy
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWUnitStoreBuyInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWUnitStoreBuyInputParam>(decrypted);

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
                DWUnitStoreBuyModel result = result = GetResult(p);

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
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWUnitStoreBuyModel GetResult(DWUnitStoreBuyInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWUnitStoreBuyModel result = new DWUnitStoreBuyModel();
            
            long enhancedStone = 0;
            long cashenhancedStone = 0;
            byte unitSlotIdx = 0;
            byte unitStore = 0;
            List<UnitStoreData> unitStoreList = null;
            Dictionary<uint, UnitData> unitList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT EnhancedStone, CashEnhancedStone, UnitSlotIdx, UnitStore, UnitStoreList, UnitList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWUnitStoreBuyController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            enhancedStone = (long)dreader[0];
                            cashenhancedStone = (long)dreader[1];
                            unitSlotIdx = (byte)dreader[2];
                            unitStore = (byte)dreader[3];
                            unitStoreList = DWMemberData.ConvertUnitStoreList(dreader[4] as byte[]);
                            unitList = DWMemberData.ConvertUnitDic(dreader[5] as byte[]);
                        }
                    }
                }
            }

            if(unitStore == 0)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("Not Open Unit Store");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            UnitSlotDataTable unitSlotDataTable = DWDataTableManager.GetDataTable(UnitSlotDataTable_List.NAME, unitSlotIdx) as UnitSlotDataTable;
            if(unitSlotDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("Not Found Unit Slot DataTable = {0}", unitSlotIdx);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if (unitSlotDataTable.UnitMaxCount == unitList.Count)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("UnitSlotDataTable MaxCount SerialNo = {0}, Cur Unit Count = {1}", unitSlotIdx, unitList.Count);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            UnitStoreData unitSlotData = unitStoreList[p.index];
            unitSlotData.count--;
            if(unitSlotData.count == 0)
            {
                unitStoreList.RemoveAt(p.index);
            }

            UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, unitSlotData.serialNo) as UnitDataTable;
            if(unitDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("Not Found Unit DataTable = {0}", unitSlotData.serialNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWUnitStoreBuyController";
            if (DWMemberData.SubEnhancedStone(ref enhancedStone, ref cashenhancedStone,  unitDataTable.UnitStoreMoney, logMessage) == false)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("Lack EnhancedStone");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            Logging.RunLog(logMessage);

            uint instanceNo = DWMemberData.AddUnitDic(ref unitList, unitSlotData.serialNo);
            UnitData unitData = null;
            if(unitList.TryGetValue(instanceNo, out unitData) == false)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitStoreBuyController";
                logMessage.Message = string.Format("Unit Add Failed");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET UnitList = @unitList, EnhancedStone = @enhancedStone, CashEnhancedStone = @cashEnhancedStone, UnitStoreList = @unitStoreList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
                    command.Parameters.Add("@enhancedStone", SqlDbType.BigInt).Value = enhancedStone;
                    command.Parameters.Add("@cashEnhancedStone", SqlDbType.BigInt).Value = cashenhancedStone;
                    command.Parameters.Add("@unitStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitStoreList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWUnitStoreBuyController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);
                        return result;
                    }
                }
            }

            result.unitData = new ClientUnitData()
            {
                instanceNo = instanceNo,
                level = 1,
                enhancementCount = 0,
                serialNo = unitData.SerialNo
            };

            result.enhancedStone = enhancedStone;
            result.cashEnhancedStone = cashenhancedStone;
            result.index = p.index;

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWUnitStoreBuyController";
            logMessage.Message = string.Format("Success Unit SerialNo = {0}, EnhancementStone = {1}, CashEnhancementStone = {2}", unitData.SerialNo, enhancedStone, cashenhancedStone);
            Logging.RunLog(logMessage);

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
