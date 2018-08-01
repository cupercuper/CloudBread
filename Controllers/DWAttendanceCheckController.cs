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
using CloudBreadRedis;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWAttendanceCheckController : ApiController
    {
        // GET api/DWAttendanceCheck
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWAttendanceCheckDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWAttendanceCheckDataInputParam>(decrypted);

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
                DWAttendanceCheckDataModel result = GetResult(p);

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

        DWAttendanceCheckDataModel GetResult(DWAttendanceCheckDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWAttendanceCheckDataModel result = new DWAttendanceCheckDataModel();

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            if (CBRedis.KeyExists(RedisIndex.ATTENDANCE_RANK_IDX, p.memberID))
            {
                result.attendanceCheck = 0;
                return result;
            }

            byte continueAttendanceCnt = 0;
            byte accAttendanceCnt = 0;
            short continueAttendanceNo = -1;
            short accAttendanceNo = -1;
            long gem = 0;
            long cashGem = 0;
            long gas = 0;
            long cashGas = 0;
            long ether = 0;
            long cashEther = 0;
            double gold = 0;
            long relicBoxCnt = 0;
            short lastWorld = 0;
            short lastStage = 0;
            List<SkillItemData> skillItemList = null;
            List<BoxData> boxList = null;

            DateTime lastAttendanceRewardTime = DateTime.UtcNow;
            int timeZoneTotalMin = 0;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT LastAttendanceRewardTime, TimeZone, ContinueAttendanceCnt, ContinueAttendanceNo, AccAttendanceCnt, AccAttendanceNo, Gem, CashGem, Gas, CashGas, Ether, CashEther, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            lastAttendanceRewardTime = (DateTime)dreader[0];
                            timeZoneTotalMin = (int)dreader[1];
                            continueAttendanceCnt = (byte)dreader[2];
                            continueAttendanceNo = (short)dreader[3];
                            accAttendanceCnt = (byte)dreader[4];
                            accAttendanceNo = (short)dreader[5];
                            gem = (long)dreader[6];
                            cashGem = (long)dreader[7];
                            gas = (long)dreader[8];
                            cashGas = (long)dreader[9];
                            ether = (long)dreader[10];
                            cashEther = (long)dreader[11];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[12] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[13] as byte[]);
                            relicBoxCnt = (long)dreader[14];
                            lastWorld = (short)dreader[15];
                            lastStage = (short)dreader[16];
                        }
                    }
                }
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            DateTime utcNow = DateTime.UtcNow;
            DateTime curUserTime = utcNow.AddMinutes((double)timeZoneTotalMin);
            DateTime nextRefreshTime = curUserTime.AddDays(1);
            nextRefreshTime = nextRefreshTime.AddHours(-nextRefreshTime.Hour);
            nextRefreshTime = nextRefreshTime.AddMinutes(-nextRefreshTime.Minute);
            nextRefreshTime = nextRefreshTime.AddSeconds(-nextRefreshTime.Second);
            nextRefreshTime = nextRefreshTime.AddMilliseconds(-nextRefreshTime.Millisecond);

            TimeSpan nextTime = nextRefreshTime - curUserTime;

            CBRedis.SetRedisExpireKey(RedisIndex.ATTENDANCE_RANK_IDX, p.memberID, p.memberID, nextTime);

            TimeSpan subTime = curUserTime - lastAttendanceRewardTime;
            // 지난 출석 보상 받은 시간보다 하루가 지나서 출석 했으면 연속 출석은 리셋
            if (subTime.TotalDays > 1 || continueAttendanceNo == -1)
            {
                continueAttendanceCnt = 0;
                continueAttendanceNo = (short)DWDataTableManager.GetContinueAttendanceTableNo();
            }

            List<DWItemData> continueAttendanceTable = DWDataTableManager.GetContinueAttendanceTable(continueAttendanceNo);
            if (continueAttendanceTable == null || continueAttendanceTable.Count <= continueAttendanceCnt)
            {
                continueAttendanceCnt = 0;
                continueAttendanceNo = (short)DWDataTableManager.GetContinueAttendanceTableNo();
                continueAttendanceTable = DWDataTableManager.GetContinueAttendanceTable(continueAttendanceNo);
            }

            DWItemData continueAttendanceRewardData = new DWItemData();
            continueAttendanceRewardData.itemType = continueAttendanceTable[continueAttendanceCnt].itemType;
            continueAttendanceRewardData.subType = continueAttendanceTable[continueAttendanceCnt].subType;
            continueAttendanceRewardData.value = continueAttendanceTable[continueAttendanceCnt].value;
            DWMemberData.AddItem(continueAttendanceRewardData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGem, ref relicBoxCnt, ref skillItemList, ref boxList, stageNo, logMessage);

            List<DWItemData> accAttendanceTable = DWDataTableManager.GetAccAttendanceTable(accAttendanceNo);
            if (accAttendanceTable == null || accAttendanceTable.Count <= accAttendanceCnt)
            {
                accAttendanceCnt = 0;
                accAttendanceNo = (short)DWDataTableManager.GetAccAttendanceTableNo();
                accAttendanceTable = DWDataTableManager.GetAccAttendanceTable(accAttendanceNo);
            }

            DWItemData accAttendanceRewardData = new DWItemData();
            accAttendanceRewardData.itemType = accAttendanceTable[accAttendanceCnt].itemType;
            accAttendanceRewardData.subType = accAttendanceTable[accAttendanceCnt].subType;
            accAttendanceRewardData.value = accAttendanceTable[accAttendanceCnt].value;

            DWMemberData.AddItem(accAttendanceRewardData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGem, ref relicBoxCnt, ref skillItemList, ref boxList, stageNo, logMessage);

            continueAttendanceCnt++;
            accAttendanceCnt++;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET LastAttendanceRewardTime = @lastAttendanceRewardTime, ContinueAttendanceCnt = @continueAttendanceCnt, ContinueAttendanceNo = @continueAttendanceNo, AccAttendanceCnt=@accAttendanceCnt, AccAttendanceNo=@accAttendanceNo, Gem=@gem, Gas=@gas, Ether=@ether, SkillItemList=@skillItemList, BoxList=@boxList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@lastAttendanceRewardTime", SqlDbType.DateTime).Value = utcNow;
                    command.Parameters.Add("@continueAttendanceCnt", SqlDbType.TinyInt).Value = continueAttendanceCnt;
                    command.Parameters.Add("@continueAttendanceNo", SqlDbType.SmallInt).Value = continueAttendanceNo;
                    command.Parameters.Add("@accAttendanceCnt", SqlDbType.TinyInt).Value = accAttendanceCnt;
                    command.Parameters.Add("@accAttendanceNo", SqlDbType.SmallInt).Value = accAttendanceNo;
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "AttendanceCheck";
                        logMessage.Message = string.Format("DWMembersNew Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.continueAttendanceCnt = continueAttendanceCnt;
            result.accAttendanceCnt = accAttendanceCnt;
            result.continueAttendanceNo = continueAttendanceNo;
            result.accAttendanceNo = accAttendanceNo;
            result.attendanceCheck = 1;
            result.ether = ether;
            result.gas = gas;
            result.gem = gem;
            result.gold = gold;
            result.skillItemList = skillItemList;
            result.boxList = boxList;

            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
