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
    public class DWRelicDestroyController : ApiController
    {
        // GET api/DWRelicDestroy
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicDestroyInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicDestroyInputParam>(decrypted);

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
                DWRelicDestroyModel result = GetResult(p);

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

        DWRelicDestroyModel GetResult(DWRelicDestroyInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicDestroyModel result = new DWRelicDestroyModel();


            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            Dictionary<uint, RelicData> relicDataStoreDic = new Dictionary<uint, RelicData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT RelicStoreList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            relicDataStoreDic = DWMemberData.ConvertRelicDataDic(dreader[0] as byte[]);
                            //relicDataDic = DWMemberData.ConvertRelicDataDic(dreader[1] as byte[]);
                        }
                    }
                }
            }

            RelicData relicData = null;
            if (relicDataStoreDic.TryGetValue(p.instanceNo, out relicData) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            relicDataStoreDic.Remove(p.instanceNo);

            //RelicDataTable relicDataTable = DWDataTableManager.GetDataTable(RelicDataTable_List.NAME, relicData.serialNo) as RelicDataTable;
            //if(relicDataTable == null)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //double relicCount = (double)relicDataDic.Count;
            //double A = (relicCount + 1.0) * Math.Pow(1.31, (relicCount + 1.0));
            //double B = ((relicCount * Math.Pow(1.31, relicCount)) + A) * 10.0;
            //double relicMoney = Math.Pow(3.2, Math.Log(B, 8)) + 43.0;

            //if (DWMemberData.SubGem(ref gem, ref cashGem, (long)relicMoney, logMessage) == false)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //RelicUpgradeDataTable upgradeDataTable = DWDataTableManager.GetDataTable(RelicUpgradeDataTable_List.NAME, relicDataTable.UpgradeTableNo) as RelicUpgradeDataTable;
            //if (upgradeDataTable == null)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //double upgradeMoneyRatio = ((double)upgradeDataTable.UpgradeMoneyRatio / 1000.0);
            //long upgradeMoney = 0;
            //for (int i = 1; i < relicData.level; ++i)
            //{
            //    long money = (long)(upgradeMoneyRatio * Math.Pow((double)((double)i + upgradeDataTable.UpgradeFirstMoney), 2.5));
            //    upgradeMoney += money;
            //}

            //if(DWMemberData.AddEther(ref ether, ref cashEther, upgradeMoney, 0, logMessage) == false)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET RelicStoreList = @relicStoreList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@relicStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataStoreDic);

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
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
