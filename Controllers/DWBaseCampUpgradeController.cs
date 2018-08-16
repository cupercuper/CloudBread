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
    public class DWBaseCampUpgradeController : ApiController
    {
        // GET api/DWBaseCampUpgrade
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBaseCampUpgradeDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBaseCampUpgradeDataInputParam>(decrypted);

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
                /// Database connection retry policy
                DWBaseCampUpgradeDataModel result = GetResult(p);

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
                logMessage.Logger = "DWGetGemController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWBaseCampUpgradeDataModel GetResult(DWBaseCampUpgradeDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWBaseCampUpgradeDataModel result = new DWBaseCampUpgradeDataModel();

            long gas = 0;
            long cashGas = 0;
            Dictionary<ulong, ushort> baseCampDic = new Dictionary<ulong, ushort>();
            List<BuffValueData> buffValueList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gas, CashGas, BaseCampList, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWGemBoxOpenController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gas = (long)dreader[0];
                            cashGas = (long)dreader[1];
                            baseCampDic = DWMemberData.ConvertBaseCampDic(dreader[2] as byte[]);
                            buffValueList = DWMemberData.ConvertBuffValueList(dreader[3] as byte[]);
                        }
                    }
                }
            }

            ushort level = 0;
            baseCampDic.TryGetValue(p.serialNo, out level);

            BaseCampDataTable baseCampDataTable = DWDataTableManager.GetDataTable(BaseCampDataTable_List.NAME, p.serialNo) as BaseCampDataTable;
            if(level == baseCampDataTable.MaxLevel)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            
            long upgradeMoney = level + 1;

            if (DWMemberData.SubGas(ref gas, ref cashGas, upgradeMoney, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ++level;
            baseCampDic.Remove(p.serialNo);
            baseCampDic.Add(p.serialNo, level);

            double prevValue = (double)(level - 1) * ((double)baseCampDataTable.BuffValue / 1000.0);
            double nextValue = (double)level * ((double)baseCampDataTable.BuffValue / 1000.0);

            DWMemberData.AddBuffValueDataList(ref buffValueList, baseCampDataTable.BuffType, prevValue, nextValue);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gas = @gas, CashGas = @cashGas, BaseCampList = @baseCampList, BuffValueList = @buffValueList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@cashGas", SqlDbType.BigInt).Value = cashGas;
                    command.Parameters.Add("@baseCampList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(baseCampDic);
                    command.Parameters.Add("@buffValueList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(buffValueList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWGetGemController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.level = level;
            result.gas = gas;
            result.cashGas = cashGas;
            result.errorCode= (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
