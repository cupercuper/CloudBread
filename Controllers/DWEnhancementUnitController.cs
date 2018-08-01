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
    public class DWEnhancementUnitController : ApiController
    {
        // GET api/DWEnhancementUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWEnhancementUnitInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWEnhancementUnitInputParam>(decrypted);

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
                DWEnhancementUnitModel result = GetResult(p);

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
                logMessage.Logger = "DWEnhancementUnitController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWEnhancementUnitModel GetResult(DWEnhancementUnitInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWEnhancementUnitModel result = new DWEnhancementUnitModel();
            //long ether = 0;
            //long cashEther = 0;

            //Dictionary<uint, UnitData> unitLIst = null;
            ///// Database connection retry policy
            //RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("SELECT Ether, CashEther, UnitList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        connection.OpenWithRetry(retryPolicy);

            //        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
            //        {
            //            if(dreader.HasRows == false)
            //            {
            //                logMessage.memberID = p.memberID;
            //                logMessage.Level = "Error";
            //                logMessage.Logger = "DWEnhancementUnitController";
            //                logMessage.Message = string.Format("Not Found User MemberID = {0}", p.memberID);
            //                Logging.RunLog(logMessage);

            //                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
            //                return result;
            //            }

            //            while (dreader.Read())
            //            {
            //                ether = (long)dreader[0];
            //                cashEther = (long)dreader[1];
            //                unitLIst = DWMemberData.ConvertUnitDic(dreader[2] as byte[]);
            //            }
            //        }
            //    }
            //}

            //UnitData unitData = null;
            //if (unitLIst.TryGetValue(p.InstanceNo, out unitData) == false)
            //{
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWEnhancementUnitController";
            //    logMessage.Message = string.Format("Not Found Unit InstanceNo = {0}", p.InstanceNo);
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //if(p.curEnhancementCnt != unitData.EnhancementCount)
            //{
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWEnhancementUnitController";
            //    logMessage.Message = string.Format("Dont Same Unit EnhancementCount server = {0}, client = {1}", unitData.EnhancementCount, p.curEnhancementCnt);
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //EnhancementDataTable enhancementDataTable = DWDataTableManager.GetDataTable(EnhancementDataTable_List.NAME, (ulong)(unitData.EnhancementCount + 1)) as EnhancementDataTable;
            //if(enhancementDataTable == null)
            //{
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWEnhancementUnitController";
            //    logMessage.Message = string.Format("Not Found EnhancementDataTable SerialNo = {0}", (unitData.EnhancementCount + 1));
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //UnitDataTable unitDataTable = DWDataTableManager.GetDataTable(UnitDataTable_List.NAME, unitData.SerialNo) as UnitDataTable;
            //if(unitDataTable == null)
            //{
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWEnhancementUnitController";
            //    logMessage.Message = string.Format("Not Found UnitDataTable SerialNo = {0}", unitData.SerialNo);
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}

            //int necessaryStoneCount = enhancementDataTable.StoneCnt;

            //logMessage.memberID = p.memberID;
            //logMessage.Level = "INFO";
            //logMessage.Logger = "DWEnhancementUnitController";
            
            //if (DWMemberData.SubEther(ref ether, ref cashEther, necessaryStoneCount, logMessage) == false)
            //{
            //    logMessage.Level = "Error";
            //    logMessage.Message = string.Format("Lack Ether cur stone = {0}, cur Cash stone = {1}, necessaryStoneCount = {2}, UnitSerial = {3}, Enhancement Serial = {4}", ether, cashEther, necessaryStoneCount, unitData.SerialNo, unitData.EnhancementCount);
            //    Logging.RunLog(logMessage);

            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    return result;
            //}
            //Logging.RunLog(logMessage);

            //unitData.EnhancementCount++;
 
            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("UPDATE DWMembersNew SET UnitList = @unitList, Ether = @ether, CashEther = @cashEther WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitLIst);
            //        command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
            //        command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;

            //        connection.OpenWithRetry(retryPolicy);

            //        int rowCount = command.ExecuteNonQuery();
            //        if (rowCount <= 0)
            //        {
            //            logMessage.memberID = p.memberID;
            //            logMessage.Level = "Error";
            //            logMessage.Logger = "DWEnhancementUnitController";
            //            logMessage.Message = string.Format("Update Failed");
            //            Logging.RunLog(logMessage);

            //            result.errorCode = (byte)DW_ERROR_CODE.OK; 
            //            return result;
            //        }
            //    }
            //}

            //logMessage.memberID = p.memberID;
            //logMessage.Level = "INFO";
            //logMessage.Logger = "DWEnhancementUnitController";
            //logMessage.Message = string.Format("InstanceNo = {0}, SerialNo = {1}, enhancementCount = {2}", p.InstanceNo, unitData.SerialNo, unitData.EnhancementCount);
            //Logging.RunLog(logMessage);

            //result.unitData = new ClientUnitData()
            //{
            //    instanceNo= p.InstanceNo,
            //    level= unitData.Level,
            //    enhancementCount = unitData.EnhancementCount,
            //    serialNo = unitData.SerialNo
            //};

            //result.ether = ether;
            //result.cashEther = cashEther;
            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
