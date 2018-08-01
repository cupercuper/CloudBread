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
    public class DWUnitSlotUpgradeController : ApiController
    {
        // GET api/DWUnitSlotUpgrade
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWUnitSlotUpgradeInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWUnitSlotUpgradeInputParam>(decrypted);

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
                DWUnitSlotUpgradeModel result = result = GetResult(p);

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
                logMessage.Logger = "DWUnitSlotUpgradeController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWUnitSlotUpgradeModel GetResult(DWUnitSlotUpgradeInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWUnitSlotUpgradeModel result = new DWUnitSlotUpgradeModel();

            long gem = 0;
            long cashGem = 0;
            byte unitSlotIdx = 1;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, UnitSlotIdx FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWUnitSlotUpgradeController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            unitSlotIdx = (byte)dreader[2];
                        }
                    }
                }
            }

            unitSlotIdx++;
            UnitSlotDataTable unitSlotDataTable = DWDataTableManager.GetDataTable(UnitSlotDataTable_List.NAME, unitSlotIdx) as UnitSlotDataTable;
            if(unitSlotDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUnitSlotUpgradeController";
                logMessage.Message = string.Format("Unit Slot Error SlotNo = {0}, Gem = {1}", unitSlotIdx, gem);
                Logging.RunLog(logMessage);
                
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWUnitSlotUpgradeController";
            if(DWMemberData.SubGem(ref gem, ref cashGem, unitSlotDataTable.UpgradeMoney, logMessage) == false)
            {
                logMessage.Level = "Error";
                logMessage.Message = string.Format("Lack Gem SlotNo = {0}, Gem = {1}, CashGem = {2}", unitSlotIdx, gem, cashGem);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;

            }
            Logging.RunLog(logMessage);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, CashGem = @cashGem, UnitSlotIdx = @unitSlotIdx WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@unitSlotIdx", SqlDbType.TinyInt).Value = unitSlotIdx;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWUnitSlotUpgradeController";
                        logMessage.Message = string.Format("UpdateFailed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWUnitSlotUpgradeController";
            logMessage.Message = string.Format("Unit Slot SlotNo = {0}, Gem = {1}", unitSlotIdx, gem);
            Logging.RunLog(logMessage);

            result.gem = gem;
            result.cashGem = cashGem;
            result.unitSlotIdx = unitSlotIdx;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
