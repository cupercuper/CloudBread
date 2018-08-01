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
    public class DWChangeUnitDeckController : ApiController
    {
        // GET api/DWChangeUnitDeck
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWChangeUnitDeckInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWChangeUnitDeckInputParam>(decrypted);

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
                DWChangeUnitDeckModel result = GetResult(p);

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
                logMessage.Logger = "DWChangeUnitDeckController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWChangeUnitDeckModel GetResult(DWChangeUnitDeckInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWChangeUnitDeckModel result = new DWChangeUnitDeckModel();

            //List<uint> unitDeckList = null;
            //Dictionary<uint, UnitData> unitList = null;
            //byte unitSlotIdx = 1;

            //RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("SELECT UnitList, UnitDeckList, UnitSlotIdx FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        connection.OpenWithRetry(retryPolicy);
            //        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
            //        {
            //            if (dreader.HasRows == false)
            //            {
            //                logMessage.memberID = p.memberID;
            //                logMessage.Level = "Error";
            //                logMessage.Logger = "DWChangeUnitDeckController";
            //                logMessage.Message = string.Format("Not Found User = {0}", p.memberID);
            //                Logging.RunLog(logMessage);

            //                result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
            //                return result;
            //            }

            //            while (dreader.Read())
            //            {
            //                unitList = DWMemberData.ConvertUnitDic(dreader[0] as byte[]);
            //                unitDeckList = DWMemberData.ConvertUnitDeckList(dreader[1] as byte[]);
            //                unitSlotIdx = (byte)dreader[2];
            //            }
            //        }
            //    }
            //}

            //if (unitList == null)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWChangeUnitDeckController";
            //    logMessage.Message = string.Format("Not Found unitList OR canBuyUnitList = {0}", p.memberID);
            //    Logging.RunLog(logMessage);

            //    return result;
            //}

            //UnitSlotDataTable unitSlotDataTable = DWDataTableManager.GetDataTable(UnitSlotDataTable_List.NAME, unitSlotIdx) as UnitSlotDataTable;
            //if (unitSlotDataTable == null)
            //{
            //    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //    logMessage.memberID = p.memberID;
            //    logMessage.Level = "Error";
            //    logMessage.Logger = "DWChangeUnitDeckController";
            //    logMessage.Message = string.Format("UnitSlotDataTable = null SerialNo = {0}", unitSlotIdx);
            //    Logging.RunLog(logMessage);
            //    return result;
            //}

            //if (p.changeType == (byte)UNIT_CHANGE_TYPE.ADD_TYPE)
            //{
            //    if (unitDeckList.Count >= unitSlotDataTable.UnitMaxCount)
            //    {
            //        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
            //        logMessage.memberID = p.memberID;
            //        logMessage.Level = "Error";
            //        logMessage.Logger = "DWChangeUnitDeckController";
            //        logMessage.Message = string.Format("UnitSlotDataTable Max  SerialNo = {0}", unitSlotIdx);
            //        Logging.RunLog(logMessage);
            //        return result;
            //    }

            //    unitDeckList.Add(p.changeInstanceNo);
            //}
            //else if (p.changeType == (byte)UNIT_CHANGE_TYPE.SUB_TYPE)
            //{
            //    int index = -1;
            //    for (int i = 0; i < unitDeckList.Count; ++i)
            //    {
            //        if (unitDeckList[i] == p.originInstanceNo)
            //        {
            //            index = i;
            //            break;
            //        }
            //    }

            //    if (index == -1)
            //    {
            //        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

            //        logMessage.memberID = p.memberID;
            //        logMessage.Level = "Error";
            //        logMessage.Logger = "DWChangeUnitDeckController";
            //        logMessage.Message = string.Format("Not Found Instance No 1 = {0}", p.memberID);
            //        Logging.RunLog(logMessage);

            //        return result;
            //    }

            //    unitDeckList.RemoveAt(index);
            //}
            //else if (p.changeType == (byte)UNIT_CHANGE_TYPE.CHANGE_TYPE)
            //{
            //    int index = -1;
            //    for (int i = 0; i < unitDeckList.Count; ++i)
            //    {
            //        if (unitDeckList[i] == p.originInstanceNo)
            //        {
            //            index = i;
            //            break;
            //        }
            //    }

            //    if (index == -1)
            //    {
            //        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

            //        logMessage.memberID = p.memberID;
            //        logMessage.Level = "Error";
            //        logMessage.Logger = "DWChangeUnitDeckController";
            //        logMessage.Message = string.Format("Not Found Instance No 2 = {0}", p.memberID);
            //        Logging.RunLog(logMessage);

            //        return result;
            //    }

            //    UnitData unitData = null;
            //    if (unitList.TryGetValue(p.changeInstanceNo, out unitData) == false)
            //    {
            //        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

            //        logMessage.memberID = p.memberID;
            //        logMessage.Level = "Error";
            //        logMessage.Logger = "DWChangeUnitDeckController";
            //        logMessage.Message = string.Format("Not Found unitList = {0}", p.memberID);
            //        Logging.RunLog(logMessage);

            //        return result;
            //    }

            //    unitDeckList[index] = p.changeInstanceNo;
            //}

            //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            //{
            //    string strQuery = string.Format("UPDATE DWMembersNew SET UnitDeckList = @unitDeckList WHERE MemberID = '{0}'", p.memberID);
            //    using (SqlCommand command = new SqlCommand(strQuery, connection))
            //    {
            //        command.Parameters.Add("@unitDeckList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitDeckList);

            //        connection.OpenWithRetry(retryPolicy);

            //        int rowCount = command.ExecuteNonQuery();
            //        if (rowCount <= 0)
            //        {
            //            result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;

            //            logMessage.memberID = p.memberID;
            //            logMessage.Level = "Error";
            //            logMessage.Logger = "DWChangeUnitDeckController";
            //            logMessage.Message = string.Format("UnitDeckList Update Failed");
            //            Logging.RunLog(logMessage);

            //            return result;
            //        }
            //    }
            //}

            result.originInstanceNo = p.originInstanceNo;
            result.changeInstanceNo = p.changeInstanceNo;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}

