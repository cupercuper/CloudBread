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

namespace CloudBread.Controllers
{
    [MobileAppController]
    public class DWReturnController : ApiController
    {
        // GET api/DWReturn
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWReturnDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWReturnDataInputParams>(decrypted);

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
                DWReturnDataModel result = GetResult(p);

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
                logMessage.Logger = "DWEventController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWReturnDataModel GetResult(DWReturnDataInputParams p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();

            DWReturnDataModel result = new DWReturnDataModel();

            short lastWorld = 0;
            short lastStage = 0;
            long gas = 0;
            long cashGas = 0;
            long ether = 0;
            long cashEther = 0;
            long captainChangeStageNo = 0;
            List<UnitData> unitList = null;
            List<BuffValueData> buffValueDataList = new List<BuffValueData>();

            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT LastWorld, LastStage, Gas, CashGas, Ether, CashEther, UnitList, CaptianChange, BuffValueList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            lastWorld = (short)dreader[0];
                            lastStage = (short)dreader[1];
                            gas = (long)dreader[2];
                            cashGas = (long)dreader[3];
                            ether = (long)dreader[4];
                            cashEther = (long)dreader[5];
                            unitList = DWMemberData.ConvertUnitDataList(dreader[6] as byte[]);
                            captainChangeStageNo = (long)dreader[7];
                            buffValueDataList = DWMemberData.ConvertBuffValueList(dreader[8] as byte[]);
                        }
                    }
                }
            }

            CaptianDataTable captainDataTable = DWDataTableManager.GetDataTable(CaptianDataTable_List.NAME, p.captainIdx) as CaptianDataTable;
            if(captainDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;
            if(stageNo < DWDataTableManager.GlobalSettingDataTable.ReturnStage)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                return result;
            }

            //long bonusEther = (long)Math.Pow(((stageNo - 20) / 12), 1.82);
            long bonusEther = (long)Math.Pow((double)stageNo / 2.0, 1.82);
            bonusEther = bonusEther < 0 ? 0 : bonusEther;

            BuffValueData etherBuffValueData = buffValueDataList.Find(a => a.type == (byte)BUFF_TYPE.RETURN_GAS);
            if (etherBuffValueData != null)
            {
                long buffEther = (long)((double)bonusEther * etherBuffValueData.value);
                bonusEther += buffEther;
            }

            DWMemberData.AddEther(ref ether, ref cashEther, bonusEther, 0, logMessage);

            long addGas = 0;
            if (captainChangeStageNo == 0)
            {
                addGas = (long)((stageNo - DWDataTableManager.GlobalSettingDataTable.ReturnStage) / 25) + 1;
                captainChangeStageNo = ((addGas - 1) * 25) + (long)DWDataTableManager.GlobalSettingDataTable.ReturnStage;
            }
            else if ((long)stageNo - captainChangeStageNo >= 25)
            {
                addGas = ((long)stageNo - captainChangeStageNo) / 25;
                captainChangeStageNo = (addGas * 50) + captainChangeStageNo;
            }

            if(addGas > 0)
            {
                BuffValueData gasBuffValueData = buffValueDataList.Find(a => a.type == (byte)BUFF_TYPE.RETURN_GAS);
                if (gasBuffValueData != null)
                {
                    long buffGas = (long)((double)addGas * gasBuffValueData.value);
                    addGas += buffGas;
                }

                DWMemberData.AddGas(ref gas, ref cashGas, addGas, 0, logMessage);
            }

            unitList.Clear();

            List<ulong> firstUnitList = DWDataTableManager.GetFirstUnitList();
            for(int i = 0; i < firstUnitList.Count; ++i)
            {
                UnitData unitData = new UnitData();
                unitData.level = 1;
                unitData.serialNo = firstUnitList[i];
                unitList.Add(unitData);
            }

            ulong returnStageNo = 1;
            double mineral = 0;
            BuffValueData returnStageBuffValueData = buffValueDataList.Find(a => a.type == (byte)BUFF_TYPE.RETURN_STAGE);
            if(returnStageBuffValueData != null)
            {
                returnStageBuffValueData.value = Math.Min(returnStageBuffValueData.value, 90.0);
                returnStageNo = (ulong)((double)stageNo * returnStageBuffValueData.value);
                returnStageNo = returnStageNo == 0 ? 1 : returnStageNo;

                // 보스 스테이지가 걸렸을경우 다음 스테이지로 바꿔 준다.
                if (returnStageNo % 10 == 0)
                {
                    returnStageNo++;
                }

                EnemyDataTable enemy = DWDataTableManager.GetDataTable(EnemyDataTable_List.NAME, DWDataTableManager.GlobalSettingDataTable.WarpMonsterID) as EnemyDataTable;
                if (enemy == null)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;
                    return result;
                }

                double mineralValue = DWDataTableManager.GlobalSettingDataTable.MonsterMineralValue / 1000.0;

                for (ulong i = 1; i < returnStageNo; ++i)
                {
                    mineral += enemy.HP * Math.Pow(1.39, Math.Min(i, 115.0)) * Math.Pow(1.13, Math.Max((i - 115.0), 0.0)) * 0.058 * mineralValue + 0.0002 * Math.Min(stageNo, 150) * 40.0;
                    mineral = Math.Truncate(mineral);
                }
            }

            List<UnitData> addUnitList = new List<UnitData>();

            Dictionary<ulong, DataTableBase> unitDic = DWDataTableManager.GetDataTableList(UnitDataTable_List.NAME);
            foreach (KeyValuePair<ulong, DataTableBase> kv in unitDic)
            {
                UnitDataTable unitDataTable = kv.Value as UnitDataTable;
                if (unitDataTable == null)
                {
                    continue;
                }

                if (unitDataTable.OpenStage <= returnStageNo)
                {
                    UnitData unitData = unitList.Find(a => a.serialNo == kv.Key);
                    if (unitData == null)
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

            ulong returnWorldNo = returnStageNo / 10;
            returnStageNo = returnStageNo - (returnWorldNo * 10);
            returnWorldNo++;

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET CurWorld = @curWorld, CurStage = @curStage, LastWorld = @lastWorld, LastStage = @lastStage, Gas = @gas, Ether = @ether, UnitList = @unitList, CaptianID = @captianID, CaptianLevel = @captianLevel, CaptianChange = @captianChange, LastReturnStage = @lastReturnStage WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = (short)returnWorldNo;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = (short)returnStageNo;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = (short)returnWorldNo;
                    command.Parameters.Add("@lastStage", SqlDbType.SmallInt).Value = (short)returnStageNo;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitList);
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = p.captainIdx;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = 1;
                    command.Parameters.Add("@captianChange", SqlDbType.BigInt).Value = captainChangeStageNo;
                    command.Parameters.Add("@lastReturnStage", SqlDbType.BigInt).Value = (long)stageNo;

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

            result.mineral = mineral;
            result.ether = ether;
            result.gas = gas;
            result.captainIdx = p.captainIdx;
            result.unitList = unitList;
            result.lastGasStageNo = captainChangeStageNo;
            result.lastReturnStageNo = (long)stageNo;
            result.returnWorldNo = (short)returnWorldNo;
            result.returnStageNo = (short)returnStageNo;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
