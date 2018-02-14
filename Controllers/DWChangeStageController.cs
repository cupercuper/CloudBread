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
using CloudBreadRedis;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWChangeStageController : ApiController
    {
        // GET api/DWChangeStage
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWChangeStageInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWChangeStageInputParam>(decrypted);

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
                DWChangeStageModel result = GetResult(p);

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
                logMessage.Logger = "DWChangeStageController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWChangeStageModel GetResult(DWChangeStageInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWChangeStageModel result = new DWChangeStageModel();
            
            short lastStageNo = 0;
            short lastWorldNo = 0;
            bool allClear = false;
            long accStageCnt = 0;
            DateTime utcTime = DateTime.UtcNow;
            DateTime gemBoxCreateTime = DateTime.UtcNow;
            bool gemBoxGet = false;
            long curGemBoxNo = 0;
            

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT LastStage, LastWorld, AllClear, AccStageCnt, GemBoxCreateTime, GemBoxGet, GemBoxNo FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWChangeStageController";
                            logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
                            Logging.RunLog(logMessage);

                            return result;
                        }

                        while (dreader.Read())
                        {
                            lastStageNo = (short)dreader[0];
                            lastWorldNo = (short)dreader[1];
                            allClear = (bool)dreader[2];
                            accStageCnt = (long)dreader[3];
                            gemBoxCreateTime = (DateTime)dreader[4];
                            gemBoxGet = (bool)dreader[5];
                            curGemBoxNo = (long)dreader[6];
                        }
                    }
                }
            }

            short worldNo = (short)(p.stageIdx / 10);

            if(p.stageIdx % 10 == 0)
            {
                worldNo = (short)(worldNo - 1);
            }

            short curStageNo = (short)(p.stageIdx - (worldNo * 10));

            if(lastWorldNo + 1 < worldNo)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWChangeStageController";
                logMessage.Message = string.Format("World Error lastWorldNo = {0}", lastWorldNo);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if (p.allClear == true)
            {
                WorldDataTable worldDataTable = DWDataTableManager.GetDataTable(WorldDataTable_List.NAME, (ulong)(lastWorldNo + 1)) as WorldDataTable;
                if(worldDataTable != null || lastStageNo % 10 != 0)
                {
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWChangeStageController";
                    logMessage.Message = string.Format("World Error lastWorldNo = {0}, lastStageNo = {1}", lastWorldNo, lastStageNo);
                    Logging.RunLog(logMessage);

                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            if (lastWorldNo < worldNo)
            {
                lastWorldNo = worldNo;
                lastStageNo = 0;
            }

            if (lastStageNo < curStageNo && lastWorldNo <= worldNo)
            {
                lastStageNo = curStageNo;
                accStageCnt++;
            }

            ulong gemBoxNo = 0;

            TimeSpan subTime = utcTime - gemBoxCreateTime;
            if (gemBoxGet == true && subTime.TotalMinutes >= DWDataTableManager.GlobalSettingDataTable.GemBoxDelay)
            {
                gemBoxGet = false;
                gemBoxCreateTime = utcTime;
                gemBoxNo = DWDataTableManager.GetGemBoxNo();
                curGemBoxNo = (long)gemBoxNo;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET CurStage = @curStage, LastStage = @lastStage, CurWorld = @curWorld, LastWorld = @lastWorld, AllClear = @allClear, AccStageCnt = @accStageCnt, GemBoxCreateTime = @gemBoxCreateTime, GemBoxGet = @gemBoxGet, GemBoxNo = @gemBoxNo WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = worldNo;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = curStageNo;
                    command.Parameters.Add("@lastStage", SqlDbType.SmallInt).Value = lastStageNo;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = lastWorldNo;
                    command.Parameters.Add("@allClear", SqlDbType.Bit).Value = p.allClear;
                    command.Parameters.Add("@accStageCnt", SqlDbType.BigInt).Value = accStageCnt;
                    command.Parameters.Add("@gemBoxCreateTime", SqlDbType.DateTime).Value = gemBoxCreateTime;
                    command.Parameters.Add("@gemBoxGet", SqlDbType.Bit).Value = gemBoxGet;
                    command.Parameters.Add("@gemBoxNo", SqlDbType.BigInt).Value = curGemBoxNo;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWChangeStageController";
                        logMessage.Message = string.Format("Update Failed MemberID = {0}", p.memberID);
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.gemBoxNo = gemBoxNo;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }

}
