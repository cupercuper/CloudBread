﻿using System;
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
using StackExchange.Redis;
using CloudBreadRedis;

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWUseGameSpeedItemController : ApiController
    {
        // GET api/DWUseGameSpeedItem
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWUseGameSpeedItemDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWUseGameSpeedItemDataInputParams>(decrypted);

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
                DWUseGameSpeedItemDataModel result = GetResult(p);

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
                logMessage.Logger = "DWLuckySupplyShipStartController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWUseGameSpeedItemDataModel GetResult(DWUseGameSpeedItemDataInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWUseGameSpeedItemDataModel result = new DWUseGameSpeedItemDataModel();
            byte gameSpeedItemCnt = 0;
            DateTime gameSpeedItemStartTime = DateTime.UtcNow;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT GameSpeedItemCount, GameSpeedItemStartTime FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            gameSpeedItemCnt = (byte)dreader[0];
                            gameSpeedItemStartTime = (DateTime)dreader[1];
                        }
                    }
                }
            }

            TimeSpan remainTime;
            if (gameSpeedItemCnt == 0)
            {
                gameSpeedItemCnt = 1;
                gameSpeedItemStartTime = DateTime.UtcNow;
                remainTime = new TimeSpan(0, DWDataTableManager.GlobalSettingDataTable.GameSpeedItemTime, 0);
            }
            else
            {
                int minutes = DWDataTableManager.GlobalSettingDataTable.GameSpeedItemTime * gameSpeedItemCnt;
                DateTime endTime = gameSpeedItemStartTime.AddMinutes(minutes);
                
                if (DateTime.UtcNow >= endTime)
                {
                    gameSpeedItemCnt = 1;
                    gameSpeedItemStartTime = DateTime.UtcNow;
                    remainTime = new TimeSpan(0, DWDataTableManager.GlobalSettingDataTable.GameSpeedItemTime, 0);
                }
                else
                {
                    if (gameSpeedItemCnt >= DWDataTableManager.GlobalSettingDataTable.GameSpeedItemMaxCount)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                        return result;
                    }

                    gameSpeedItemCnt++;
                    minutes = DWDataTableManager.GlobalSettingDataTable.GameSpeedItemTime * gameSpeedItemCnt;
                    endTime = gameSpeedItemStartTime.AddMinutes(minutes);
                    remainTime = endTime - DateTime.UtcNow;
                }
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET GameSpeedItemCount = @gameSpeedItemCount, GameSpeedItemStartTime=@gameSpeedItemStartTime WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gameSpeedItemCount", SqlDbType.TinyInt).Value = gameSpeedItemCnt;
                    command.Parameters.Add("@gameSpeedItemStartTime", SqlDbType.DateTime).Value = gameSpeedItemStartTime;

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

            result.gameSpeedItemCnt = gameSpeedItemCnt;
            result.remainTime = remainTime.Ticks;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
