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

            List<UnitData> unitList = null;
            short lastWorld = 0, lastStage = 0;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT UnitList, LastWorld, LastStage FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            unitList = DWMemberData.ConvertUnitDataList(dreader[0] as byte[]);
                            lastWorld = (short)dreader[1];
                            lastStage = (short)dreader[2];
                        }
                    }
                }
            }

            if (unitList == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found unitList OR canBuyUnitList = {0}", p.memberID);
                Logging.RunLog(logMessage);

                return result;
            }

            UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, p.serialNo) as UnitDataTable;
            if(unitDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found unitList OR canBuyUnitList = {0}", p.memberID);
                Logging.RunLog(logMessage);

                return result;
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            if(stageNo < unitDataTable.OpenStage)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWBuyUnitController";
                logMessage.Message = string.Format("Not Found unitList OR canBuyUnitList = {0}", p.memberID);
                Logging.RunLog(logMessage);

                return result;
            }

            UnitData unitData = new UnitData();
            unitData.level = p.level;
            unitData.serialNo = p.serialNo;

            unitList.Add(unitData);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET UnitList = @unitList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);

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
            
            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWBuyUnitController";
            logMessage.Message = string.Format("UnitSerialNo = {0}, UnitLevel = {1}", p.serialNo, p.level);
            Logging.RunLog(logMessage);

            result.unitData = unitData;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;  

        }
    }
}
