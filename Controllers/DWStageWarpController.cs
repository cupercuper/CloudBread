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
    public class DWStageWarpController : ApiController
    {
        // GET api/DWStageWarp
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWStageWarpDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWStageWarpDataInputParam>(decrypted);

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
                DWStageWarpDataModel result = GetResult(p);

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
                logMessage.Logger = "DWGemBoxOpenController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWStageWarpDataModel GetResult(DWStageWarpDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWStageWarpDataModel result = new DWStageWarpDataModel();

            long gem = 0;
            long cashGem = 0;
            long lastReturnStageNo = 0;
            short lastWorld = 0;
            short lastStage = 0;
            List<UnitData> unitList = new List<UnitData>();
            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, LastReturnStage, LastWorld, LastStage, UnitList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    connection.OpenWithRetry(retryPolicy);

                    using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                    {
                        if (dreader.HasRows == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.NOT_FOUND_USER;
                            return result;
                        }

                        while (dreader.Read())
                        {
                            gem = (long)dreader[0];
                            cashGem = (long)dreader[1];
                            lastReturnStageNo = (long)dreader[2];
                            lastWorld = (short)dreader[3];
                            lastStage = (short)dreader[4];
                            unitList = DWMemberData.ConvertUnitDataList(dreader[5] as byte[]);
                        }
                    }
                }
            }

            if(lastReturnStageNo == 0)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            WarpDataTable warpDataTable = DWDataTableManager.GetDataTable(WarpDataTable_List.NAME, (ulong)p.warpIdx) as WarpDataTable;
            if(warpDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ulong stageNo = ((ulong)lastWorld - 1) * 10 + (ulong)lastStage;
            double warpValue = (double)warpDataTable.Value / 1000.0;
            ulong warpStageNo = (ulong)((double)lastReturnStageNo * warpValue);
            
            // 보스 스테이지가 걸렸을경우 다음 스테이지로 바꿔 준다.
            if(warpStageNo % 10 == 0)
            {
                warpStageNo++;
            }

            if(stageNo >= warpStageNo)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            if(warpDataTable.GemCount != 0)
            {
                if(DWMemberData.SubGem(ref gem, ref cashGem, warpDataTable.GemCount, logMessage) == false)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }
            }

            List<UnitData> addUnitList = new List<UnitData>();

            Dictionary<ulong, DataTableBase> unitDic = DWDataTableManager.GetDataTableList(UnitDataTable_List.NAME);
            foreach(KeyValuePair<ulong, DataTableBase> kv in unitDic)
            {
                UnitDataTable unitDataTable = kv.Value as UnitDataTable;
                if(unitDataTable == null)
                {
                    continue;
                }

                if(unitDataTable.OpenStage <= warpStageNo)
                {
                    UnitData unitData = unitList.Find(a => a.serialNo == kv.Key);
                    if(unitData == null)
                    {
                        unitData = new UnitData();
                        unitData.serialNo = kv.Key;
                        unitData.level = 1;
                        addUnitList.Add(unitData);
                    }
                }
            }

            if (addUnitList.Count > 0)
            {
                unitList.AddRange(addUnitList.ToArray());
            }

            ulong warpWorldNo = warpStageNo / 10;
            warpStageNo = warpStageNo - (warpWorldNo * 10);
            warpWorldNo++;

            EnemyDataTable enemy = DWDataTableManager.GetDataTable(EnemyDataTable_List.NAME, DWDataTableManager.GlobalSettingDataTable.WarpMonsterID) as EnemyDataTable;
            if(enemy == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            double mineral = 0;
            ulong subStage = warpStageNo - stageNo;
            for(ulong i = 1; i <= subStage; ++i)
            {
                double mineralStageNo = stageNo + (double)i;
                mineral += enemy.HP* Math.Pow(1.39, Math.Min(mineralStageNo, 115.0)) * Math.Pow(1.13, Math.Max((mineralStageNo - 115.0), 9.0));
            }
            
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, CashGem = @cashGem, LastWorld = @lastWorld, LastStage = @lastStage, CurWorld = @curWorld, CurStage = @curStage, UnitList = @unitList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = (short)warpWorldNo;
                    command.Parameters.Add("@lastStage", SqlDbType.SmallInt).Value = (short)warpStageNo;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = (short)warpWorldNo;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = (short)warpStageNo;
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
     
                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWGemBoxOpenController";
                        logMessage.Message = string.Format("Update Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.mineral = mineral;
            result.gem = gem;
            result.cashGem = cashGem;
            result.warpWorldNo = (short)warpWorldNo;
            result.warpStageNo = (short)warpStageNo;
            result.addUnitList = addUnitList;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
