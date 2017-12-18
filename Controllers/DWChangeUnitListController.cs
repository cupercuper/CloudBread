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
    public class DWChangeUnitListController : ApiController
    {
        // GET api/DWChangeUnitList
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWChangeUnitListInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWChangeUnitListInputParam>(decrypted);

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
                DWChangeUnitListModel result = GetResult(p);

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
                logMessage.Logger = "DWChangeUnitListController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWChangeUnitListModel GetResult(DWChangeUnitListInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWChangeUnitListModel result = new DWChangeUnitListModel();
            long gem = 0;
            long cashGem = 0;
            DateTime utcTime = DateTime.UtcNow;
            DateTime unitListChangeTime = DateTime.UtcNow;
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, UnitListChangeTime FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if(dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "INFO";
                            logMessage.Logger = "DWChangeUnitListController";
                            logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            unitListChangeTime = (DateTime)dreader[2];
                        }
                    } 
                }
            }

            GlobalSettingDataTable globalSetting = DWDataTableManager.GetDataTable(GlobalSettingDataTable_List.NAME, 1) as GlobalSettingDataTable;
            if(globalSetting == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWChangeUnitListController";
                logMessage.Message = string.Format("Not Found GlobalSettingDataTable SerialNo = 1");
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            // 2분을 갭을 준다.
            DateTime addChangeTime = unitListChangeTime.AddMinutes((double)(globalSetting.UnitListChangeTime - 2));
            if (addChangeTime > utcTime)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWChangeUnitListController";
                if (DWMemberData.SubGem(ref gem, ref cashGem, globalSetting.UnitListChangeGem, logMessage) == false)
                {
                    logMessage.Message = string.Format("Lack Gem");
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
                Logging.RunLog(logMessage);
            }

            List<ulong> unitLIst = DWDataTableManager.GetCanBuyUnitList();
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET CanBuyUnitList = @canBuyUnitList, Gem = @gem, CashGem = @cashGem, UnitListChangeTime = @unitListChangeTime WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitLIst);
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@unitListChangeTime", SqlDbType.DateTime).Value = utcTime;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWChangeUnitListController";
                        logMessage.Message = string.Format("Update failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            logMessage.memberID = p.memberID;
            logMessage.Level = "INFO";
            logMessage.Logger = "DWChangeUnitListController";
            logMessage.Message = string.Format("Cur Gem = {0}, Cur CashGem = {1}", gem, cashGem);
            Logging.RunLog(logMessage);

            result.unitList = unitLIst;
            result.gem = gem;
            result.cashGem = cashGem;
            result.unitListChangeTime = utcTime.Ticks;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
