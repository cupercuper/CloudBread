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
using CloudBread.Manager;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWOpenResourceDrillController : ApiController
    {
        // GET api/DWOpenResourceDrill
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWOpenResouceDrillDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWOpenResouceDrillDataInputParams>(decrypted);

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
                DWOpenResouceDrillDataModel result = result = GetResult(p);

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
                logMessage.Logger = "DWOpenResourceDrillController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWOpenResouceDrillDataModel GetResult(DWOpenResouceDrillDataInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWOpenResouceDrillDataModel result = new DWOpenResouceDrillDataModel();
            byte resouceDrillIdx = 0;
            DateTime resouceDrillStartTime = DateTime.UtcNow;
            double gold = 0;
            long gem = 0;
            long cashGem = 0;
            long ether = 0;
            long cashEther = 0;
            long gas = 0;
            long cashGas = 0;
            List<SkillItemData> skillItemList = null;
            List<BoxData> boxList = null;
            long relicBoxCnt = 0;
            short lastWorld = 0;
            short lastStage = 0;
            bool droneAdvertisingOff = false;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT ResouceDrillIdx, ResouceDrillStartTime, Gem, CashGem, Ether, CashEther, Gas, CashGas, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage, DroneAdvertisingOff FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWOpenResourceDrillController";
                            logMessage.Message = string.Format("Select Failed");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            resouceDrillIdx =  (byte)dreader[0];
                            resouceDrillStartTime = (DateTime)dreader[1];
                            gem = (long)dreader[2];
                            cashGem = (long)dreader[3];
                            ether = (long)dreader[4];
                            cashEther = (long)dreader[5];
                            gas = (long)dreader[6];
                            cashGas = (long)dreader[7];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[8] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[9] as byte[]);
                            relicBoxCnt = (long)dreader[8];
                            lastWorld = (short)dreader[9];
                            lastStage = (short)dreader[10];
                            droneAdvertisingOff = (bool)dreader[11];
                        }
                    }
                }
            }

            // 같은 인덱스가 날아 올 수 없다.
            if(resouceDrillIdx == p.drillIdx)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWOpenResourceDrillController";
                logMessage.Message = string.Format("Drill Idx Same {0} = {1}", resouceDrillIdx, p.drillIdx);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            if (resouceDrillIdx > 0)
            {
                if(resouceDrillIdx + 1 < p.drillIdx)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWOpenResourceDrillController";
                    logMessage.Message = string.Format("Drill Idx Over {0} = {1}", resouceDrillIdx, p.drillIdx);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                ResourceDrillDataTable drillDataTable = DWDataTableManager.GetDataTable(ResourceDrillDataTable_List.NAME, resouceDrillIdx) as ResourceDrillDataTable;
                if(drillDataTable == null)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWOpenResourceDrillController";
                    logMessage.Message = string.Format("Cur Drill DataTable null Idx = {0}", resouceDrillIdx);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                TimeSpan subTime = DateTime.UtcNow - resouceDrillStartTime;
                // 아직 시간이 안끝난 상황에서 기존 drill 인덱스 보다 작은 인덱스가 오면 해킹
                if(subTime.TotalHours < (double)drillDataTable.ResetTime && resouceDrillIdx >= p.drillIdx)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWOpenResourceDrillController";
                    logMessage.Message = string.Format("Drill TIme Faile orogin={0}, client={1}, remainTime={2}, tableTime={3} ",resouceDrillIdx, p.drillIdx, subTime.ToString(), drillDataTable.ResetTime.ToString());
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
                // 리셋 시간이 다 지난 상황에서 drill 인덱스가 1보다 크면 해킹 무조건 1부터 시작
                else if(p.drillIdx > 1)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWOpenResourceDrillController";
                    logMessage.Message = string.Format("Drill Refresh Idx Over origin={0}, client={1}", resouceDrillIdx, p.drillIdx);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }
            else
            {
                if(p.drillIdx > 1)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWOpenResourceDrillController";
                    logMessage.Message = string.Format("Drill Idx Over client={1}", p.drillIdx);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            ResourceDrillDataTable nextDrillDataTable = DWDataTableManager.GetDataTable(ResourceDrillDataTable_List.NAME, p.drillIdx) as ResourceDrillDataTable;
            if (nextDrillDataTable == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWOpenResourceDrillController";
                logMessage.Message = string.Format("Next Drill DataTable null client={1}", p.drillIdx);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }
            // 아이템 지급
            DWItemData itemData = new DWItemData();
            itemData.itemType = nextDrillDataTable.ItemType;
            itemData.subType = nextDrillDataTable.ItemSubType;
            itemData.value = nextDrillDataTable.ItemValue;
            DWMemberData.AddItem(itemData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, ref droneAdvertisingOff, stageNo, logMessage);
            //--------------------------------------------------------------   
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET ResouceDrillIdx = @resouceDrillIdx, ResouceDrillStartTime = @resouceDrillStartTime, Gem = @gem, Ether = @ether, Gas = @gas, SkillItemList = @skillItemList, BoxList = @boxList, DroneAdvertisingOff = @droneAdvertisingOff WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@resouceDrillIdx", SqlDbType.TinyInt).Value = p.drillIdx;
                    command.Parameters.Add("@resouceDrillStartTime", SqlDbType.DateTime).Value = DateTime.UtcNow;

                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);
                    command.Parameters.Add("@droneAdvertisingOff", SqlDbType.Bit).Value = droneAdvertisingOff;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWGetRewardDailyQuestController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
