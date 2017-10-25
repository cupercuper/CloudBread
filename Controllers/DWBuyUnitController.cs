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

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWBuyUnitController : ApiController
    {
        // GET api/DWBuyUnit
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWBuyUnitInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWBuyUnitInputParam>(decrypted);

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
                DWBuyUnitModel result = GetResult(p);

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
                logMessage.Logger = "DWGetUserDataController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWBuyUnitModel GetResult(DWBuyUnitInputParam p)
        {
            DWBuyUnitModel result = new DWBuyUnitModel();

            Dictionary<uint, UnitData> unitList = null;
            List<ulong> canBuyUnitList = null;
            int gem = 0;
            int enhancedStone = 0;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT UnitList, CanBuyUnitList, Gem, EnhancedStone FROM DWMembers WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);
                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if(dreader.HasRows == false)
                        {
                            result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            unitList = DWMemberData.ConvertUnitDic(dreader[0] as byte[]);
                            canBuyUnitList = DWMemberData.ConvertUnitList(dreader[1] as byte[]);
                            gem = (int)dreader[2];
                            enhancedStone = (int)dreader[3];
                        }
                    }
                }
            }

            if (unitList == null || canBuyUnitList == null)
            {
                result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            if (canBuyUnitList.Count == 0 || canBuyUnitList.Count <= p.index || p.index < 0)
            {
                result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            ulong serialNo = canBuyUnitList[p.index];
            UnitSummonDataTable unitSummonDataTable = DWDataTableManager.GetDataTable(UnitSummonDataTable_List.NAME, serialNo) as UnitSummonDataTable;
            if (unitSummonDataTable == null)
            {
                result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            switch ((MONEY_TYPE)unitSummonDataTable.BuyType)
            {
                case MONEY_TYPE.ENHANCEMENT_TYPE:
                    if (enhancedStone < unitSummonDataTable.BuyCount)
                    {
                        result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                        return result;
                    }
                    else
                    {
                        enhancedStone -= unitSummonDataTable.BuyCount;
                    }
                    break;
                case MONEY_TYPE.GEM_TYPE:
                    if (gem < unitSummonDataTable.BuyCount)
                    {
                        result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                        return result;
                    }
                    else
                    {
                        gem -= unitSummonDataTable.BuyCount;
                    }
                    break;
            }

            canBuyUnitList.RemoveAt(p.index);

            uint instanceNo = DWMemberData.AddUnitDic(ref unitList, unitSummonDataTable.ChangeSerialNo);
            UnitData unitData = null;
            if (unitList.TryGetValue(instanceNo, out unitData) == false)
            {
                result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                return result;
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembers SET UnitList = @unitList, CanBuyUnitList = @canBuyUnitList, Gem = @gem, EnhancedStone = @enhancedStone WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
                    command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(canBuyUnitList);
                    command.Parameters.Add("@gem", SqlDbType.Int).Value = gem;
                    command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = enhancedStone;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                        return result;
                    }
                }
            }
            
            result.UnitData = unitData;
            result.Gem = gem;
            result.EnhancedStone = enhancedStone;
            result.ErrorCode = (byte)DW_ERROR_CODE.OK;

            return result;  

        }
    }
}
