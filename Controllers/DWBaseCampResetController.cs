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
    public class DWBaseCampResetController : ApiController
    {
        // GET api/DWBaseCampReset
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBaseCampResetDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBaseCampResetDataInputParam>(decrypted);

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
                DWBaseCampResetDataModel result = GetResult(p);

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

        DWBaseCampResetDataModel GetResult(DWBaseCampResetDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWBaseCampResetDataModel result = new DWBaseCampResetDataModel();

            long gas = 0;
            long cashGas = 0;
            long gem = 0;
            long cashGem = 0;
            Dictionary<ulong, ushort> baseCampDic = new Dictionary<ulong, ushort>();
            long resetCnt = 0;
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gas, CashGas, Gem, CashGem, BaseCampList, BaseCampResetCount, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            gem = (long)dreader[2];
                            cashGem = (long)dreader[3];
                            baseCampDic = DWMemberData.ConvertBaseCampDic(dreader[4] as byte[]);
                            resetCnt = (long)dreader[5];
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[6] as byte[]);
                        }
                    }
                }
            }

            if (long.MaxValue - resetCnt > 0)
            {
                resetCnt++;
            }

            if (resetCnt > 1)
            {
                long resetMoney = Math.Min(resetCnt * (resetCnt - 1) * 100, 1000);
                if(DWMemberData.SubGem(ref gem, ref cashGem, resetMoney, logMessage) == false)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            long totalGas = 0;
            foreach (KeyValuePair<ulong, ushort> kv in baseCampDic)
            {
                ushort level = kv.Value;

                BaseCampDataTable baseCampDataTable = DWDataTableManager.GetDataTable(BaseCampDataTable_List.NAME, kv.Key) as BaseCampDataTable;
                if(baseCampDataTable == null)
                {
                    continue;
                }
                
                double value = (double)level * ((double)baseCampDataTable.BuffValue / 1000.0);
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, baseCampDataTable.BuffType, value, 0.0);

                for (int i = 0; i < level; ++i)
                {
                    totalGas += i + 1;
                }
            }

            if (DWMemberData.AddGas(ref gas, ref cashGas, totalGas, 0, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            baseCampDic.Clear();

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gas = @gas, CashGas = @cashGas, Gem = @gem, CashGem = @cashGem, BaseCampList = @baseCampList, BaseCampResetCount = @baseCampResetCount, BuffValueList = @buffValueList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@cashGas", SqlDbType.BigInt).Value = cashGas;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@baseCampList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(baseCampDic);
                    command.Parameters.Add("@baseCampResetCount", SqlDbType.BigInt).Value = resetCnt;
                    command.Parameters.Add("@buffValueList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(buffValueDataList);

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

            result.gas = gas;
            result.cashGas = cashGas;
            result.gem = gem;
            result.cashGem = cashGem;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
