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
    public class DWRelicOpenController : ApiController
    {
        // GET api/DWRelicOpen
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicOpenInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicOpenInputParam>(decrypted);

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
                DWRelicOpenModel result = GetResult(p);

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

        DWRelicOpenModel GetResult(DWRelicOpenInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicOpenModel result = new DWRelicOpenModel();

            long ether = 0;
            long cashEther = 0;
            byte relicSlotIdx = 0;
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Ether, CashEther, RelicSlotIdx, RelicList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            relicSlotIdx = (byte)dreader[2];
                            relicDataDic = DWMemberData.ConvertRelicDataDic(dreader[3] as byte []);
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

            if(relicDataDic.Count >= slotDataTable.Count)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            double relicCount = (double)relicDataDic.Count;
            double A = (relicCount + 1.0) * Math.Pow(1.31, (relicCount + 1.0));
            double B = ((relicCount * Math.Pow(1.31, relicCount)) + A) * 10.0;
            double relicMoney = Math.Pow(3.2, Math.Log(B, 8));

            if(DWMemberData.SubEther(ref ether, ref cashEther, (long)relicMoney, logMessage) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ulong relicNo = RelicNo(relicDataDic);
            if (relicNo == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }


            RelicDataTable relicDataTable = DWDataTableManager.GetDataTable(RelicDataTable_List.NAME, relicNo) as RelicDataTable;
            if(relicDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            uint instanceNo = 0;
            if (DWMemberData.InsertRelicInstanceNo(relicDataDic, out instanceNo) == false)
            {
                result.errorCode = (byte)DW_ERROR_CODE.INSTANCE_NO_OVER;
                return result;
            }

            Random random = new Random((int)DateTime.Now.Ticks);
            List<double> buffValueList = new List<double>();
            if(relicDataTable.Buff_1 != 0)
            {
                buffValueList.Add(DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_1, relicDataTable.BuffMaxValue_1));
            }

            if (relicDataTable.Buff_2 != 0)
            {
                buffValueList.Add(DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_2, relicDataTable.BuffMaxValue_2));
            }

            if (relicDataTable.Buff_3 != 0)
            {
                buffValueList.Add(DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_3, relicDataTable.BuffMaxValue_3));
            }

            RelicData relicData = new RelicData();
            relicData.instanceNo = instanceNo;
            relicData.level = 1;
            relicData.serialNo = relicNo;
            relicData.buffValue = buffValueList;

            relicDataDic.Add(instanceNo, relicData);

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Ether = @ether, CashEther = @cashEther, RelicList = @relicList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataDic);

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

            result.relicData = relicData;
            result.ether = ether;
            result.cashEther = cashEther;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }

        ulong RelicNo(Dictionary<uint, RelicData> curRelicDic)
        {
            double maxRate = 1.0;

            List<DWDataTableManager.RelicRatioData> ratioList = new List<DWDataTableManager.RelicRatioData>();
            ratioList.AddRange(DWDataTableManager.GetRelicRatioList().ToArray());

            foreach (KeyValuePair<uint, RelicData> kv in curRelicDic)
            {
                DWDataTableManager.RelicRatioData ratioData = ratioList.Find(a => a.SerialNo == kv.Value.serialNo);
                maxRate -= ratioData.Ratio;
                ratioList.Remove(ratioData);
            }

            Random random = new Random((int)DateTime.Now.Ticks);
            double num = random.NextDouble() * maxRate;
            for (int i = 0; i < ratioList.Count; ++i)
            {
                num -= ratioList[i].Ratio;
                if (num <= 0.0)
                {
                    return ratioList[i].SerialNo;
                }
            }

            return ratioList[ratioList.Count - 1].SerialNo;
        }
    }

}
