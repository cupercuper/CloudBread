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
    public class DWRelicUpgradeController : ApiController
    {
        // GET api/DWRelicUpgrade
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicUpgradeInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicUpgradeInputParam>(decrypted);

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
                DWRelicUpgradeModel result = GetResult(p);

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

        DWRelicUpgradeModel GetResult(DWRelicUpgradeInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicUpgradeModel result = new DWRelicUpgradeModel();

            long ether = 0;
            long cashEther = 0;
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Ether, CashEther, RelicList, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            relicDataDic = DWMemberData.ConvertRelicDataDic(dreader[2] as byte[]);
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[3] as byte[]);
                        }
                    }
                }
            }

            RelicData relicData = null;
            if(relicDataDic.TryGetValue(p.instanceNo, out relicData) == false)
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

            if(relicDataTable.MaxLevel == relicData.level || relicDataTable.MaxLevel <= relicData.level + p.levelCnt)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            RelicUpgradeDataTable upgradeDataTable = DWDataTableManager.GetDataTable(RelicUpgradeDataTable_List.NAME, relicDataTable.UpgradeTableNo) as RelicUpgradeDataTable;
            if(upgradeDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            double upgradeMoneyRatio = ((double)upgradeDataTable.UpgradeMoneyRatio / 1000.0);
            double upgradeMoney = 0;
            for (int i = 0; i < p.levelCnt; ++i)
            {
                long money = (long)(upgradeMoneyRatio * Math.Pow((double)((relicData.level + i) + upgradeDataTable.UpgradeFirstMoney), 2.5));
                upgradeMoney += money;
            }

            if(DWMemberData.SubEther(ref ether, ref cashEther, (long)upgradeMoney, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ushort prevLevel = relicData.level;
            relicData.level += p.levelCnt;
            ushort nextLevel = relicData.level;

            if (relicDataTable.Buff_1 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_1 / 1000.0;
                double prevValue = relicData.buffValue[0] + ((prevLevel - 1) * ratio);
                double nextValue = relicData.buffValue[0] + ((nextLevel - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_1, prevValue, nextValue);
            }

            if (relicDataTable.Buff_2 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_2 / 1000.0;
                double prevValue = relicData.buffValue[1] + ((prevLevel - 1) * ratio);
                double nextValue = relicData.buffValue[1] + ((nextLevel - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_2, prevValue, nextValue);
            }

            if (relicDataTable.Buff_3 != 0)
            {
                double ratio = relicDataTable.BuffLevelRatio_3 / 1000.0;
                double prevValue = relicData.buffValue[2] + ((prevLevel - 1) * ratio);
                double nextValue = relicData.buffValue[2] + ((nextLevel - 1) * ratio);

                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_3, prevValue, nextValue);
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Ether = @ether, CashEther = @cashEther, RelicList = @relicList, BuffValueList = @buffValueList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataDic);
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

            result.instanceNo = p.instanceNo;
            result.level = relicData.level;
            result.ether = ether;
            result.cashEther = cashEther;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
