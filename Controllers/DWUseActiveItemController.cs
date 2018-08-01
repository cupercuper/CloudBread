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
    public class DWUseActiveItemController : ApiController
    {
        // GET api/DWUseActiveItem
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWUseActiveItemInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWUseActiveItemInputParam>(decrypted);

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
                DWUseActiveItemModel result = result = GetResult(p);

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
                logMessage.Logger = "DWUseActiveItemController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWUseActiveItemModel GetResult(DWUseActiveItemInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            DWUseActiveItemModel result = new DWUseActiveItemModel();

            long gem = 0;
            long cashGem = 0;
            List<ActiveItemData> activeItemList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, ActiveItemList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            activeItemList = DWMemberData.ConvertActiveItemList(dreader[2] as byte[]);
                        }
                    }
                }
            }

            DWMemberData.UpdateActiveItem(activeItemList);


            ActiveItemDataTable activeItemDataTable = DWDataTableManager.GetDataTable(ActiveItemDataTable_List.NAME, p.serialNo) as ActiveItemDataTable;
            if (activeItemDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWUseActiveItemController";
                logMessage.Message = string.Format("Not Fount ActiveItemDataTable serialNp = {0}", p.serialNo);
                Logging.RunLog(logMessage);

                return result;
            }

            if((MONEY_TYPE)activeItemDataTable.MoneyType == MONEY_TYPE.GEM_TYPE)
            {
                logMessage.memberID = p.memberID;
                if (DWMemberData.SubGem(ref gem, ref cashGem, activeItemDataTable.MoneyCount, logMessage) == false)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWUseActiveItemController";
                    logMessage.Message = string.Format("gem lack");
                    Logging.RunLog(logMessage);

                    return result;
                }
                logMessage.Level = "INFO";
                logMessage.Logger = "DWUseActiveItemController";
                Logging.RunLog(logMessage);
            }

            DWMemberData.AddActiveItem(activeItemList, p.serialNo);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, CashGem = @cashGem, ActiveItemList = @activeItemList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@activeItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(activeItemList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWUseActiveItemController";
                        logMessage.Message = string.Format("DWMembersNew Udpate Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }
            
            result.gem = gem;
            result.cashGem = cashGem;
            result.activeItemList = activeItemList;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
