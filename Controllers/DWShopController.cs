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
    public class DWShopController : ApiController
    {
        // GET api/DWShop
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWShopInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWShopInputParam>(decrypted);

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
                DWShopModel result = result = GetResult(p);

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
                logMessage.Logger = "DWShopController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWShopModel GetResult(DWShopInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            DWShopModel result = new DWShopModel();

            double gold = 0;
            long gem = 0;
            long cashGem = 0;
            long ether = 0;
            long cashEther = 0;
            long gas = 0;
            long cashGas = 0;
            long relicBoxCnt = 0;
            short lastWorld = 0;
            short lastStage = 0;
            List<SkillItemData> skillItemList = null;
            List<BoxData> boxList = null;
            List<LimitShopItemData> limitShopItemDataList = null;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("SELECT Gem, CashGem, Ether, CashEther, Gas, CashGas, SkillItemList, BoxList, RelicBoxCount, LastWorld, LastStage, LimitShopItemDataList FROM DWMembersNew WHERE MemberID = '{0}'", p.memberID);
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
                            ether = (long)dreader[2];
                            cashEther = (long)dreader[3];
                            gas = (long)dreader[4];
                            cashGas = (long)dreader[5];
                            skillItemList = DWMemberData.ConvertSkillItemList(dreader[6] as byte[]);
                            boxList = DWMemberData.ConvertBoxDataList(dreader[7] as byte[]);
                            relicBoxCnt = (long)dreader[8];
                            lastWorld = (short)dreader[9];
                            lastStage = (short)dreader[10];
                            limitShopItemDataList = DWMemberData.ConvertLimitShopItemDataList(dreader[11] as byte[]);
                        }
                    }
                }
            }


            ShopDataTable shopDataTable = DWDataTableManager.GetDataTable(ShopDataTable_List.NAME, p.serialNo) as ShopDataTable;
            if (shopDataTable == null)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWShopController";
                logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                Logging.RunLog(logMessage);

                return result;
            }

            if (shopDataTable.Limit > 0)
            {
                bool addItem = true;
                for (int k = 0; k < limitShopItemDataList.Count; ++k)
                {
                    if (limitShopItemDataList[k].serialNo == p.serialNo)
                    {
                        addItem = false;
                        if (shopDataTable.Limit <= limitShopItemDataList[k].count)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWGooglePurchaseVerifyController";
                            logMessage.Message = string.Format("limitItem Error Cur Count = {0} ShopDataTable SeralNo = {1}", limitShopItemDataList[k].count, p.serialNo);
                            Logging.RunLog(logMessage);

                            return result;
                        }
                        else
                        {
                            limitShopItemDataList[k].count++;
                        }
                    }
                }

                if (addItem)
                {
                    LimitShopItemData limitShopItemData = new LimitShopItemData();
                    limitShopItemData.serialNo = p.serialNo;
                    limitShopItemData.count = 1;
                    limitShopItemDataList.Add(limitShopItemData);
                }
            }

            if ((MONEY_TYPE)shopDataTable.MoneyType == MONEY_TYPE.CASH_TYPE)
            {
                result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                logMessage.memberID = p.memberID;
                logMessage.Level = "Error";
                logMessage.Logger = "DWShopController";
                logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.serialNo);
                Logging.RunLog(logMessage);

                return result;
            }

            switch((MONEY_TYPE)shopDataTable.MoneyType)
            {
                case MONEY_TYPE.GEM_TYPE:
                    if (DWMemberData.SubGem(ref gem, ref cashGem, shopDataTable.MoneyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWShopController";
                        logMessage.Message = string.Format("lack gem ({0},{1}) shop serialNo = {2} moneyCount = {3}", gem, cashGem, p.serialNo, shopDataTable.MoneyCount);
                        Logging.RunLog(logMessage);

                        return result;
                    }
                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWShopController";
                    Logging.RunLog(logMessage);
                    break;
                case MONEY_TYPE.ETHER_TYPE:
                    if (DWMemberData.SubEther(ref ether, ref cashEther, shopDataTable.MoneyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWShopController";
                        logMessage.Message = string.Format("lack Ether ({0},{1}) shop serialNo = {2} moneyCount = {3}", ether, cashEther, p.serialNo, shopDataTable.MoneyCount);
                        Logging.RunLog(logMessage);

                        return result;
                    }

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWShopController";
                    Logging.RunLog(logMessage);

                    break;
                case MONEY_TYPE.GAS_TYPE:
                    if (DWMemberData.SubGas(ref gas, ref cashGas, shopDataTable.MoneyCount, logMessage) == false)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWShopController";
                        logMessage.Message = string.Format("lack Ether ({0},{1}) shop serialNo = {2} moneyCount = {3}", ether, cashEther, p.serialNo, shopDataTable.MoneyCount);
                        Logging.RunLog(logMessage);

                        return result;
                    }

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "INFO";
                    logMessage.Logger = "DWShopController";
                    Logging.RunLog(logMessage);

                    break;
            }

            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;
            DWItemData itemData = new DWItemData();
            for (int i = 0; i < shopDataTable.ItemTypeList.Count; ++i)
            {
                itemData.itemType = shopDataTable.ItemTypeList[i];
                itemData.subType = shopDataTable.ItemSubTypeList[i];
                itemData.value = shopDataTable.ItemValueList[i];

                DWMemberData.AddItem(itemData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, stageNo, logMessage);
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, CashGem = @cashGem, Ether = @ether, CashEther = @cashEther, Gas = @gas, CashGas = @cashGas, SkillItemList = @skillItemList, BoxList=@boxList, RelicBoxCount=@relicBoxCount, LimitShopItemDataList=@limitShopItemDataList WHERE MemberID = '{0}'", p.memberID);
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@gem", SqlDbType.BigInt).Value = gem;
                    command.Parameters.Add("@cashGem", SqlDbType.BigInt).Value = cashGem;
                    command.Parameters.Add("@ether", SqlDbType.BigInt).Value = ether;
                    command.Parameters.Add("@cashEther", SqlDbType.BigInt).Value = cashEther;
                    command.Parameters.Add("@gas", SqlDbType.BigInt).Value = gas;
                    command.Parameters.Add("@cashGas", SqlDbType.BigInt).Value = cashGas;
                    command.Parameters.Add("@skillItemList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(skillItemList);
                    command.Parameters.Add("@boxList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(boxList);
                    command.Parameters.Add("@relicBoxCount", SqlDbType.BigInt).Value = relicBoxCnt;
                    command.Parameters.Add("@limitShopItemDataList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(limitShopItemDataList);

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        logMessage.memberID = p.memberID;
                        logMessage.Level = "Error";
                        logMessage.Logger = "DWShopController";
                        logMessage.Message = string.Format("DWMembersNew Udpate Failed");
                        Logging.RunLog(logMessage);

                        result.errorCode = (byte)DW_ERROR_CODE.DB_ERROR;
                        return result;
                    }
                }
            }

            result.gold = gold;
            result.gem = gem;
            result.cashGem = cashGem;
            result.ether = ether;
            result.cashEther = cashEther;
            result.gas = gas;
            result.cashGas = cashGas;
            result.skillItemList = skillItemList;
            result.boxList = boxList;
            result.relicBoxCnt = relicBoxCnt;
            result.limitShopItemDataList = limitShopItemDataList;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }
    }
}
