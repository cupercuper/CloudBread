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
    public class DWEnhancementResetController : ApiController
    {
        // GET api/DWEnhancementReset
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWEnhancementResetInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWEnhancementResetInputParam>(decrypted);

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
                DWEnhancementResetModel result = GetResult(p);

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
                logMessage.Logger = "DWEnhancementResetController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWEnhancementResetModel GetResult(DWEnhancementResetInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWEnhancementResetModel result = new DWEnhancementResetModel();

            //// Database connection retry policy
            //Dictionary<uint, UnitData> unitList = null;
            //long ether = 0;
            //long cashEther = 0;


            //RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("SELECT UnitList, Ether, CashEther FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        connection.OpenWithRetry(retryPolicy);
            //        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
            //        {
            //            if (dreader.HasRows == false)
            //            {
            //                logMessage.memberID = p.memberID;
            //                logMessage.Level = "Error";
            //                logMessage.Logger = "DWEnhancementResetController";
            //                logMessage.Message = string.Format("Not Found User");
            //                Logging.RunLog(logMessage);

            //                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
            //                return result;
            //            }

            //            while (dreader.Read())
            //            {
            //                unitList = DWMemberData.ConvertUnitDic(dreader[0] as byte[]);
            //                ether = (long)dreader[1];
            //                cashEther = (long)dreader[2];
            //            }
            //        }
            //    }
            //}

            //UnitData unitData = null;
            //if (unitList.TryGetValue(p.instanceNo, out unitData) == false)
            //{
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWEnhancementResetController";
            //    logMessage.Message = string.Format("Not Found Unit Instance = {0}", p.instanceNo);
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //long stoneCnt = 0;
            //EnhancementDataTable enhancementDataTable = DWDataTableManager.GetDataTable(EnhancementDataTable_List.NAME, (ulong)unitData.EnhancementCount) as EnhancementDataTable;
            //if (enhancementDataTable != null)
            //{
            //    stoneCnt = (long)enhancementDataTable.AccStoneCnt;
            //}

            //unitData.EnhancementCount = 0;

            //logMessage.memberID = p.memberID;
            //logMessage.Level = "Info";
            //logMessage.Logger = "DWEnhancementResetController";

            //DWMemberData.AddEther(ref ether, ref cashEther, stoneCnt, 0, logMessage);
            //Logging.RunLog(logMessage);

            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("UPDATE DWMembersNew SET UnitList = @unitList, Ether = @ether WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
            //        command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;

            //        connection.OpenWithRetry(retryPolicy);

            //        int rowCount = command.ExecuteNonQuery();
            //        if (rowCount <= 0)
            //        {
            //            logMessage.memberID = p.memberID;
            //            logMessage.Level = "Error";
            //            logMessage.Logger = "DWEnhancementResetController";
            //            logMessage.Message = string.Format("Update Failed");
            //            Logging.RunLog(logMessage);

            //            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
            //            return result;
            //        }
            //    }
            //}

            //result.instanceNo = p.instanceNo;
            //result.ether = stoneCnt;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
