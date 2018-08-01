using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudBreadRedis;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using CloudBread.globals;
using System.Data.SqlClient;
using DW.CommonData;
using Logger.Logging;
using System.Data;

namespace CloudBread.Manager
{
    public class AttendanceCheckManager
    {

        //public static void AttendanceCheckInit(string memberID, int timeZoneTotalMin, out short continueAttendanceNo, out short accAttendanceNo)
        //{
        //    DateTime utcNow = DateTime.UtcNow;
        //    DateTime curUserTime = utcNow.AddMinutes((double)timeZoneTotalMin);
        //    DateTime nextRefreshTime = curUserTime.AddDays(1);
        //    TimeSpan subTime = new TimeSpan(0, nextRefreshTime.Hour, nextRefreshTime.Minute, nextRefreshTime.Second, nextRefreshTime.Millisecond);
        //    nextRefreshTime = nextRefreshTime.Subtract(subTime);

        //    CBRedis.SetRedisExpireKey(RedisIndex.ATTENDANCE_RANK_IDX, memberID, memberID, nextRefreshTime - curUserTime);

        //    continueAttendanceNo = (short)DWDataTableManager.GetContinueAttendanceTableNo();
        //    accAttendanceNo = (short)DWDataTableManager.GetAccAttendanceTableNo();
        //}

        //public static bool AttendanceCheck(string memberID, out byte continueAttendanceCnt, out short continueAttendanceNo, out byte accAttendanceCnt, out short accAttendanceNo)
        //{
        //    continueAttendanceCnt = 0;
        //    accAttendanceCnt = 0;
        //    continueAttendanceNo = -1;
        //    accAttendanceNo = -1;

        //    if (CBRedis.KeyExists(RedisIndex.ATTENDANCE_RANK_IDX, memberID))
        //    {
        //        return false;
        //    }

        //    DateTime lastAttendanceRewardTime = DateTime.UtcNow;
        //    int timeZoneTotalMin = 0;

        //    RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
        //    using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
        //    {
        //        string strQuery = string.Format("SELECT LastAttendanceRewardTime, TimeZone, ContinueAttendanceCnt, ContinueAttendanceNo, AccAttendanceCnt, AccAttendanceNo  FROM DWMembersNew WHERE MemberID = '{0}'", memberID);
        //        using (SqlCommand command = new SqlCommand(strQuery, connection))
        //        {
        //            connection.OpenWithRetry(retryPolicy);
        //            using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
        //            {
        //                if (dreader.HasRows == false)
        //                {
        //                    return false;
        //                }

        //                while (dreader.Read())
        //                {
        //                    lastAttendanceRewardTime = (DateTime)dreader[0];
        //                    timeZoneTotalMin = (int)dreader[1];
        //                    continueAttendanceCnt = (byte)dreader[2];
        //                    continueAttendanceNo = (short)dreader[3];
        //                    accAttendanceCnt = (byte)dreader[4];
        //                    accAttendanceNo = (short)dreader[5];
        //                }
        //            }
        //        }
        //    }

        //    DateTime utcNow = DateTime.UtcNow;
        //    DateTime curUserTime = utcNow.AddMinutes((double)timeZoneTotalMin);
        //    DateTime nextRefreshTime = curUserTime.AddDays(1);
        //    nextRefreshTime.AddHours(-nextRefreshTime.Hour);
        //    nextRefreshTime.AddMinutes(-nextRefreshTime.Minute);
        //    nextRefreshTime.AddSeconds(-nextRefreshTime.Second);
        //    nextRefreshTime.AddMilliseconds(-nextRefreshTime.Millisecond);
        
        //    CBRedis.SetRedisExpireKey(RedisIndex.ATTENDANCE_RANK_IDX, memberID, memberID, nextRefreshTime - curUserTime);

        //    TimeSpan subTime = curUserTime - lastAttendanceRewardTime;
        //    // 지난 출석 보상 받은 시간보다 하루가 지나서 출석 했으면 연속 출석은 리셋
        //    if(subTime.TotalDays > 1 || continueAttendanceNo == -1)
        //    {
        //        continueAttendanceCnt = 0;
        //        continueAttendanceNo = (short)DWDataTableManager.GetContinueAttendanceTableNo();
        //    }

        //    List<DWDataTableManager.AttendanceRewardData> continueAttendanceTable  = DWDataTableManager.GetContinueAttendanceTable(continueAttendanceNo);
        //    if(continueAttendanceTable == null || continueAttendanceTable.Count <= continueAttendanceCnt)
        //    {
        //        continueAttendanceCnt = 0;
        //        continueAttendanceNo = (short)DWDataTableManager.GetContinueAttendanceTableNo();
        //        continueAttendanceTable = DWDataTableManager.GetContinueAttendanceTable(continueAttendanceNo);
        //    }

        //    DWDataTableManager.AttendanceRewardData continueAttendanceRewardData = continueAttendanceTable[continueAttendanceCnt];
        //    //AddMail(memberID, "연속 출석 보상", string.Format("{0} 일 연속 출석 보상", continueAttendanceCnt + 1), continueAttendanceRewardData);

        //    List<DWDataTableManager.AttendanceRewardData> accAttendanceTable = DWDataTableManager.GetAccAttendanceTable(accAttendanceNo);
        //    if (accAttendanceTable == null || accAttendanceTable.Count <= accAttendanceCnt)
        //    {
        //        accAttendanceCnt = 0;
        //        accAttendanceNo = (short)DWDataTableManager.GetAccAttendanceTableNo();
        //        accAttendanceTable = DWDataTableManager.GetAccAttendanceTable(accAttendanceNo);
        //    }

        //    DWDataTableManager.AttendanceRewardData accAttendanceRewardData = accAttendanceTable[accAttendanceCnt];
        //    //AddMail(memberID, "누적 출석 보상", string.Format("{0} 일 누적 출석 보상", accAttendanceCnt + 1), accAttendanceRewardData);

        //    continueAttendanceCnt++;
        //    accAttendanceCnt++;

        //    using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
        //    {
        //        string strQuery = string.Format("UPDATE DWMembersNew SET LastAttendanceRewardTime = @lastAttendanceRewardTime, ContinueAttendanceCnt = @continueAttendanceCnt, ContinueAttendanceNo = @continueAttendanceNo, AccAttendanceCnt=@accAttendanceCnt, AccAttendanceNo=@accAttendanceNo WHERE MemberID = '{0}'", memberID);
        //        using (SqlCommand command = new SqlCommand(strQuery, connection))
        //        {
        //            command.Parameters.Add("@lastAttendanceRewardTime", SqlDbType.DateTime).Value = utcNow;
        //            command.Parameters.Add("@continueAttendanceCnt", SqlDbType.TinyInt).Value = continueAttendanceCnt;
        //            command.Parameters.Add("@continueAttendanceNo", SqlDbType.SmallInt).Value = continueAttendanceNo;
        //            command.Parameters.Add("@accAttendanceCnt", SqlDbType.TinyInt).Value = accAttendanceCnt;
        //            command.Parameters.Add("@accAttendanceNo", SqlDbType.SmallInt).Value = accAttendanceNo;

        //            connection.OpenWithRetry(retryPolicy);

        //            int rowCount = command.ExecuteNonQuery();
        //            if (rowCount <= 0)
        //            {
        //                Logging.CBLoggers logMessage = new Logging.CBLoggers();

        //                logMessage.memberID = memberID;
        //                logMessage.Level = "Error";
        //                logMessage.Logger = "AttendanceCheck";
        //                logMessage.Message = string.Format("DWMembersNew Update Failed");
        //                Logging.RunLog(logMessage);

        //                return false;
        //            }
        //        }
        //    }


        //    return true;
        //}

        //static void AddMail(string receiveID, string title, string msg, DWDataTableManager.AttendanceRewardData rewardData)
        //{
        //    DWMailData mailData = new DWMailData();
        //    mailData.title = title;
        //    mailData.msg = msg;

        //    DWItemData itemData = new DWItemData();
        //    itemData.itemType = rewardData.ItemType;
        //    itemData.value = rewardData.itemValue;

        //    mailData.itemData = new List<DWItemData>();
        //    mailData.itemData.Add(itemData);

        //    RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
        //    using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
        //    {
        //        string strQuery = "Insert into DWMail (SenderID, ReceiveID, MailData) VALUES (@senderID, @receiveID, @mailData)";
        //        using (SqlCommand command = new SqlCommand(strQuery, connection))
        //        {
        //            command.Parameters.Add("@senderID", SqlDbType.NVarChar).Value = "Master";
        //            command.Parameters.Add("@receiveID", SqlDbType.NVarChar).Value = receiveID;
        //            command.Parameters.Add("@mailData", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(mailData);

        //            connection.OpenWithRetry(retryPolicy);

        //            int rowCount = command.ExecuteNonQuery();
        //            if (rowCount <= 0)
        //            {
        //                Logging.CBLoggers logMessage = new Logging.CBLoggers();
        //                logMessage.memberID = receiveID;
        //                logMessage.Level = "Error";
        //                logMessage.Logger = "AttendanceCheck";
        //                logMessage.Message = string.Format("Mail Insert Failed(title = {0}, msg = {1})", title, msg);
        //                Logging.RunLog(logMessage);
        //            }
        //        }
        //    }
        //}
    }
}