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
    public class DWRelicBuyController : ApiController
    {
        // GET api/DWRelicBuy
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicBuyDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicBuyDataInputParam>(decrypted);

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
                DWRelicBuyDataModel result = GetResult(p);

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

        DWRelicBuyDataModel GetResult(DWRelicBuyDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicBuyDataModel result = new DWRelicBuyDataModel();
            
            byte relicSlotIdx = 0;
            long ether = 0;
            long cashEther = 0;
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            Dictionary<uint, RelicData> relicDataStoreDic = new Dictionary<uint, RelicData>();
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT RelicSlotIdx, RelicList, RelicStoreList, Ether, CashEther, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            relicSlotIdx = (byte)dreader[0];
                            relicDataDic = DWMemberData.ConvertRelicDataDic(dreader[1] as byte[]);
                            relicDataStoreDic = DWMemberData.ConvertRelicDataDic(dreader[2] as byte[]);
                            ether = (long)dreader[3];
                            cashEther = (long)dreader[4];
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[5] as byte[]);
                        }
                    }
                }
            }

            RelicSlotDataTable slotDataTable = DWDataTableManager.GetDataTable(RelicSlotDataTable_List.NAME, (ulong)relicSlotIdx) as RelicSlotDataTable;
            if(slotDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if(slotDataTable.Count <= relicDataDic.Count)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            RelicData relicStoreData = null;
            if (relicDataStoreDic.TryGetValue(p.instanceNo, out relicStoreData) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            double relicCount = (double)relicDataDic.Count;
            double A = (relicCount + 1.0) * Math.Pow(1.31, (relicCount + 1.0));
            double B = ((relicCount * Math.Pow(1.31, relicCount)) + A) * 10.0;
            double relicMoney = Math.Pow(3.2, Math.Log(B, 8));

            if (DWMemberData.SubEther(ref ether, ref cashEther, (long)relicMoney, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            relicDataStoreDic.Remove(p.instanceNo);

            foreach(KeyValuePair<uint, RelicData> kv in relicDataDic)
            {
                if(kv.Value.serialNo == relicStoreData.serialNo)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            uint instanceNo = 0;
            if (DWMemberData.InsertRelicInstanceNo(relicDataDic, out instanceNo) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.INSTANCE_NO_OVER;
                return result;
            }

            relicStoreData.instanceNo = instanceNo;
            relicDataDic.Add(instanceNo, relicStoreData);

            RelicDataTable relicDataTable = DWDataTableManager.GetDataTable(RelicDataTable_List.NAME, relicStoreData.serialNo) as RelicDataTable;
            if (relicDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if (relicDataTable.Buff_1 != 0)
            {
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_1, 0.0, relicStoreData.buffValue[0]);
            }

            if (relicDataTable.Buff_2 != 0)
            {
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_2, 0.0, relicStoreData.buffValue[1]);
            }

            if (relicDataTable.Buff_3 != 0)
            {
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_3, 0.0, relicStoreData.buffValue[2]);
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET RelicList = @relicList, RelicStoreList = @relicStoreList, Ether = @ether, CashEther = @cashEther, BuffValueList = @buffValueList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataDic);
                    command.Parameters.Add("@relicStoreList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataStoreDic);
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;
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
            result.cashEther = cashEther;
            result.inputInstanceNo = p.instanceNo;
            result.relicListData = relicStoreData;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
