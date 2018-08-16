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
    public class DWRelicBoxOpenController : ApiController
    {
        // GET api/DWRelicBoxOpen
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWRelicBoxOpenDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWRelicBoxOpenDataInputParam>(decrypted);

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
                DWRelicBoxOpenDataModel result = GetResult(p);

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

        DWRelicBoxOpenDataModel GetResult(DWRelicBoxOpenDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWRelicBoxOpenDataModel result = new DWRelicBoxOpenDataModel();
            
            byte relicSlotIdx = 0;
            Dictionary<uint, RelicData> relicDataDic = new Dictionary<uint, RelicData>();
            long relicBoxCount = 0;
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT RelicSlotIdx, RelicList, RelicBoxCount, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            relicBoxCount = (long)dreader[2];
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[3] as byte[]);
                        }
                    }
                }
            }

            if(relicBoxCount == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            RelicSlotDataTable slotDataTable = DWDataTableManager.GetDataTable(RelicSlotDataTable_List.NAME, (ulong)relicSlotIdx) as RelicSlotDataTable;
            if (slotDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if (relicDataDic.Count >= slotDataTable.Count)
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
            if (relicDataTable == null)
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
            if (relicDataTable.Buff_1 != 0)
            {
                double value = DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_1, relicDataTable.BuffMaxValue_1);
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_1, 0.0, value);
                buffValueList.Add(value);
            }

            if (relicDataTable.Buff_2 != 0)
            {
                double value = DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_2, relicDataTable.BuffMaxValue_2);
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_2, 0.0, value);
                buffValueList.Add(value);
            }

            if (relicDataTable.Buff_3 != 0)
            {
                double value = DWMemberData.GetBuffValue(random, relicDataTable.BuffMinValue_3, relicDataTable.BuffMaxValue_3);
                DWMemberData.AddBuffValueDataList(ref buffValueDataList, relicDataTable.Buff_3, 0.0, value);
                buffValueList.Add(value);
            }

            RelicData relicData = new RelicData();
            relicData.instanceNo = instanceNo;
            relicData.level = 1;
            relicData.serialNo = relicNo;
            relicData.buffValue = buffValueList;

            relicDataDic.Add(instanceNo, relicData);

            --relicBoxCount;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET RelicList = @relicList, RelicBoxCount = @relicBoxCount, BuffValueList = @buffValueList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@relicList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(relicDataDic);
                    command.Parameters.Add("@relicBoxCount", SqlDbType.BigInt).Value = relicBoxCount;
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

            result.relicData = relicData;
            result.relicBoxCount = relicBoxCount;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }

        ulong RelicNo(Dictionary<uint, RelicData> curRelicDic)
        {
            double maxRate = 1.0;

            List<DWDataTableManager.RelicRatioData> ratioList = new List<DWDataTableManager.RelicRatioData>();
            ratioList.AddRange(DWDataTableManager.GetRelicRatioList().ToArray());

            foreach(KeyValuePair<uint, RelicData> kv in curRelicDic)
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
