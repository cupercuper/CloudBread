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
    public class DWReadMailController : ApiController
    {
        // GET api/DWReadMail
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWReadMailInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWReadMailInputParams>(decrypted);

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
                DWReadMailModel result = result = GetResult(p);

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
                logMessage.Logger = "DWReadMailController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }
        
        DWReadMailModel GetResult(DWReadMailInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWReadMailModel result = new DWReadMailModel();

            DWMailData mailData = null;
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "SELECT MailData FROM[dbo].[DWMail] Where ReceiveID = @receiveID AND [Index] = @index AND [Read] = 0";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@receiveID", SqlDbType.NVarChar).Value = p.memberID;
                    command.Parameters.Add("@index", SqlDbType.BigInt).Value = p.index;

                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        while (dreader.Read())
                        {
                            mailData = DWMemberData.ConvertMailData(dreader[0] as byte[]);
                        }
                    }
                }
            }

            if(mailData == null)
            {
                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWReadMailController";
                logMessage.Message = string.Format("MailData null Index = {0}", p.index);
                Logging.RunLog(logMessage);

                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                return result;
            }

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

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, Ether, CashEther, Gas, CashGas, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage, DroneAdvertisingOff FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWReadMailController";
                            logMessage.Message = string.Format("Not Found User");
                            Logging.RunLog(logMessage);

                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            ether = (long)dreader[2];
                            cashEther = (long)dreader[3];
                            gas = (long)dreader[4];
                            cashGas = (long)dreader[5];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[6] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[7] as byte[]);
                            relicBoxCnt = (long)dreader[8];
                            lastWorld = (short)dreader[9];
                            lastStage = (short)dreader[10];
                            droneAdvertisingOff = (bool)dreader[11];
                        }
                    }
                }
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;

            for (int i = 0; i < mailData.itemData.Count; ++i)
            {
                result.itemList.Add(mailData.itemData[i]);
                DWMemberData.AddItem(mailData.itemData[i], ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, ref droneAdvertisingOff, stageNo, logMessage);
                logMessage.memberID = p.memberID;
                logMessage.Level = "INFO";
                logMessage.Logger = "DWReadMailController";
                logMessage.Message = string.Format("Add Item itemType = {0} itemValue = {1}", mailData.itemData[i].itemType, mailData.itemData[i].value);
                Logging.RunLog(logMessage);

            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, Ether = @ether, Gas = @gas, SkillItemList = @skillItemList, BoxList = @boxList, DroneAdvertisingOff = @droneAdvertisingOff, RelicBoxCount = @relicBoxCount  WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);
                    command.Parameters.Add("@droneAdvertisingOff", SqlDbType.Bit).Value = droneAdvertisingOff;
                    command.Parameters.Add("@relicBoxCount", SqlDbType.BigInt).Value = relicBoxCnt;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWReadMailController";
                        logMessage.Message = string.Format("Update Failed DWMembersNew");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMail SET [Read] = 1, [ReadAt] = @readAt WHERE [Index] = @index");
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@readAt", SqlDbType.DateTime).Value = DateTime.UtcNow;
                    command.Parameters.Add("@index", SqlDbType.BigInt).Value = p.index;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWReadMailController";
                        logMessage.Message = string.Format("Update Failed DWMail");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.gold = gold;
            result.gem = gem;
            result.ether = ether;
            result.gas = gas;
            result.skillItemList = skillItemList;
            result.boxList = boxList;
            result.relicBoxCnt = relicBoxCnt;
            result.droneAdvertisingOff = droneAdvertisingOff;
            result.index = p.index;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }


}
