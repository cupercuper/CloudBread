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
    public class DWEnhancementUnitController : ApiController
    {
        // GET api/DWEnhancementUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWEnhancementUnitInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWEnhancementUnitInputParam>(decrypted);

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
                DWEnhancementUnitModel result = GetResult(p);

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
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWEnhancementUnitModel GetResult(DWEnhancementUnitInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWEnhancementUnitModel result = new DWEnhancementUnitModel();
            int enhancedStone = 0;
            int gem = 0;
            Dictionary<uint, UnitData> unitLIst = null;
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT EnhancedStone, Gem, UnitList FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if(dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWEnhancementUnitController";
                            logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            enhancedStone = (int)dreader[0];
                            gem = (int)dreader[1];
                            unitLIst = DWMemberData.ConvertUnitDic(dreader[2] as byte[]);
                        }
                    }
                }
            }

            UnitData unitData = null;
            if (unitLIst.TryGetValue(p.InstanceNo, out unitData) == false)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Not Found Unit InstanceNo = {0}", p.InstanceNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            EnhancementDataTable enhancementDataTable = DWDataTableManager.GetDataTable(EnhancementDataTable_List.NAME, (ulong)(unitData.EnhancementCount + 1)) as EnhancementDataTable;
            if(enhancementDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Not Found EnhancementDataTable SerialNo = {0}", (unitData.EnhancementCount + 1));
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, unitData.SerialNo) as UnitDataTable;
            if(unitDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Not Found UnitDataTable SerialNo = {0}", unitData.SerialNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            int necessaryStoneCount = 0;
            int necessaryGemCount = 0;
            switch (unitDataTable.Grade)
            {
                case 1:
                    necessaryStoneCount = enhancementDataTable.Grade_1;
                    necessaryGemCount = enhancementDataTable.ProbabilityUp_Grade_1;
                    break;
                case 2:
                    necessaryStoneCount = enhancementDataTable.Grade_2;
                    necessaryGemCount = enhancementDataTable.ProbabilityUp_Grade_2;
                    break;
                case 3:
                    necessaryStoneCount = enhancementDataTable.Grade_3;
                    necessaryGemCount = enhancementDataTable.ProbabilityUp_Grade_3;
                    break;
                case 4:
                    necessaryStoneCount = enhancementDataTable.Grade_4;
                    necessaryGemCount = enhancementDataTable.ProbabilityUp_Grade_4;
                    break;
                case 5:
                    necessaryStoneCount = enhancementDataTable.Grade_5;
                    necessaryGemCount = enhancementDataTable.ProbabilityUp_Grade_5;
                    break;
            }

            if(enhancedStone < necessaryStoneCount)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Lack enhancedStone cur stone = {0}, necessaryStoneCount = {1}, UnitSerial = {2}, Enhancement Serial = {3}", enhancedStone, necessaryStoneCount, unitData.SerialNo, (unitData.EnhancementCount + 1));
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            
            if (p.gemUse == 1 && gem < necessaryGemCount)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Lack gem cur Gem = {0}, necessaryGemCount = {1}, UnitSerial = {2}, Enhancement Serial = {3}", gem, necessaryGemCount, unitData.SerialNo, (unitData.EnhancementCount + 1));
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            GlobalSettingDataTable globalSettingDataTable = DWDataTableManager.GetDataTable(GlobalSettingDataTable_List.NAME, 1) as GlobalSettingDataTable;
            if(globalSettingDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = string.Format("Not Found GlobalSettingDataTable");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            Random rand = new Random((int)DateTime.Now.Ticks);
            int probability = rand.Next(0, 101);
            int addProbability = p.gemUse == 1 ? globalSettingDataTable.GemUseAddProbability : 0;

            if (probability <= enhancementDataTable.Probability + addProbability)
            {
                result.success = 1;
                unitData.EnhancementCount++;
            }
            else
            {
                if(p.gemUse == 0)
                {
                    unitData.EnhancementCount = (ushort)(unitData.EnhancementCount - enhancementDataTable.FailSub);
                }
                result.success = 0;
            }

            enhancedStone -= necessaryStoneCount;
            if(p.gemUse == 1)
            {
                gem -= necessaryGemCount;
            }
            
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET UnitList = @unitList, EnhancedStone = @enhancedStone, Gem = @gem WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitLIst);
                    command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = enhancedStone;
                    command.Parameters.Add("@gem", SqlDbType.Int).Value = gem;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWEnhancementUnitController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.OK; 
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWEnhancementUnitController";
            logMessage.Message = string.Format("InstanceNo = {0}, SerialNo = {1}, enhancementCount = {2}", p.InstanceNo, unitData.SerialNo, unitData.EnhancementCount);
            Logging.RunLog(logMessage);

            result.unitData = new ClientUnitData()
            {
                instanceNo= p.InstanceNo,
                level= unitData.Level,
                enhancementCount = unitData.EnhancementCount,
                serialNo = unitData.SerialNo
            };
            result.enhancedStone = enhancedStone;
            result.gem = gem;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
