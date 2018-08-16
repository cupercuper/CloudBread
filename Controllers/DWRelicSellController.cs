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
    public class DWRelicSellController : ApiController
    {
        // GET api/DWRelicSell
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicSellInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicSellInputParam>(decrypted);

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
                DWRelicSellModel result = GetResult(p);

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

        DWRelicSellModel GetResult(DWRelicSellInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicSellModel result = new DWRelicSellModel();

            long ether = 0;
            long cashEther = 0;
            long gem = 0;
            long cashGem = 0;
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            Dictionary<uint, RelicData> relicDataStoreDic = new Dictionary<uint, RelicData>();
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Ether, CashEther, Gem, CashGem, RelicList, RelicStoreList, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            ether = (long)dreader[0];
                            cashEther = (long)dreader[1];
                            gem = (long)dreader[2];
                            cashGem = (long)dreader[3];
                            relicDataDic = DWMemberData.ConvertRelicDataDic(dreader[4] as byte[]);
                            relicDataStoreDic = DWMemberData.ConvertRelicDataDic(dreader[5] as byte[]);
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[6] as byte[]);
                        }
                    }
                }
            }


            RelicData relicData = null;
            if (relicDataDic.TryGetValue(p.instanceNo, out relicData) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            long sellGemMoney = Math.Min(Math.Max((long)relicDataDic.Count * 10, 10), 1000);
            if (DWMemberData.SubGem(ref gem, ref cashGem, sellGemMoney, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            RelicDataTable relicDataTable = DWDataTableManager.GetDataTable(RelicDataTable_List.NAME, relicData.serialNo) as RelicDataTable;
            if (relicDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            RelicUpgradeDataTable upgradeDataTable = DWDataTableManager.GetDataTable(RelicUpgradeDataTable_List.NAME, relicDataTable.UpgradeTableNo) as RelicUpgradeDataTable;
            if (upgradeDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            double upgradeMoneyRatio = ((double)upgradeDataTable.UpgradeMoneyRatio / 1000.0);
            long upgradeMoney = 0;
            for (int i = 0; i < relicData.level; ++i)
            {
                long money = (long)(upgradeMoneyRatio * Math.Pow((double)((double)i + upgradeDataTable.UpgradeFirstMoney), 2.4));
                upgradeMoney += money;
            }

            if (DWMemberData.AddEther(ref ether, ref cashEther, upgradeMoney, 0, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            relicDataDic.Remove(p.instanceNo);

            uint instanceNo = 0;
            if(DWMemberData.InsertRelicInstanceNo(relicDataStoreDic, out instanceNo) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if (relicDataTable.Buff_1 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_1 / 1000.0;
                double value = relicData.buffValue[0] + ((relicData.level - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_1, value, 0.0);
            }

            if (relicDataTable.Buff_2 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_2 / 1000.0;
                double value = relicData.buffValue[1] + ((relicData.level - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_2, value, 0.0);
            }

            if (relicDataTable.Buff_3 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_3 / 1000.0;
                double value = relicData.buffValue[2] + ((relicData.level - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_3, value, 0.0);
            }

            relicData.level = 1;
            relicData.instanceNo = instanceNo;
            relicDataStoreDic.Add(instanceNo, relicData);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET RelicList = @relicList, RelicStoreList = @relicStoreList, Ether = @ether, CashEther = @cashEther, Gem = @gem, CashGem = @cashGem, BuffValueList = @buffValueLis WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataDic);
                    command.Parameters.Add("@relicStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataStoreDic);
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
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

            result.ether = ether;
            result.gem = gem;
            result.cashGem = cashGem;
            result.inputInstanceNo = p.instanceNo;
            result.relicStoreData = relicData;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
