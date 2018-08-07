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
    public class DWOpenBoxController : ApiController
    {
        // GET api/DWOpenBox
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWOpenBoxDataInputParam p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWOpenBoxDataInputParam>(decrypted);

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
                DWOpenBoxDataModel result = result = GetResult(p);

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

        DWOpenBoxDataModel GetResult(DWOpenBoxDataInputParam p)
        {
            Logging.CBLoggers logMessage = new Logging.CBLoggers();
            DWOpenBoxDataModel result = new DWOpenBoxDataModel();

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
            bool droneAdvertisingOff = false;

            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
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

            BoxData boxData = boxList.Find(a => a.type == p.boxType);
            if (boxData != null && boxData.count > 0)
            {
                boxData.count--;
            }
            else
            {
                ShopDataTable shopDataTable = DWDataTableManager.GetDataTable(ShopDataTable_List.NAME, p.shopNo) as ShopDataTable;
                if (shopDataTable == null)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWShopController";
                    logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.shopNo);
                    Logging.RunLog(logMessage);

                    return result;
                }

                if ((MONEY_TYPE)shopDataTable.MoneyType == MONEY_TYPE.CASH_TYPE)
                {
                    result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                    logMessage.memberID = p.memberID;
                    logMessage.Level = "Error";
                    logMessage.Logger = "DWShopController";
                    logMessage.Message = string.Format("Not Fount ShopDataTable serialNp = {0}", p.shopNo);
                    Logging.RunLog(logMessage);

                    return result;
                }

                switch ((MONEY_TYPE)shopDataTable.MoneyType)
                {
                    case MONEY_TYPE.GEM_TYPE:
                        if (DWMemberData.SubGem(ref gem, ref cashGem, shopDataTable.MoneyCount, logMessage) == false)
                        {
                            result.errorCode = (byte)DW_ERROR_CODE.LOGIC_ERROR;

                            logMessage.memberID = p.memberID;
                            logMessage.Level = "Error";
                            logMessage.Logger = "DWShopController";
                            logMessage.Message = string.Format("lack gem ({0},{1}) shop serialNo = {2} moneyCount = {3}", gem, cashGem, p.shopNo, shopDataTable.MoneyCount);
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
                            logMessage.Message = string.Format("lack Ether ({0},{1}) shop serialNo = {2} moneyCount = {3}", ether, cashEther, p.shopNo, shopDataTable.MoneyCount);
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
                            logMessage.Message = string.Format("lack Ether ({0},{1}) shop serialNo = {2} moneyCount = {3}", ether, cashEther, p.shopNo, shopDataTable.MoneyCount);
                            Logging.RunLog(logMessage);

                            return result;
                        }

                        logMessage.memberID = p.memberID;
                        logMessage.Level = "INFO";
                        logMessage.Logger = "DWShopController";
                        Logging.RunLog(logMessage);

                        break;
                }
            }

            int boxCount = p.boxType == (byte)BOX_TYPE.NORMAL_TYPE ? 2 : 3;
            ulong stageNo = (((ulong)lastWorld - 1) * 10) + (ulong)lastStage;
            result.itemList = new List<DWItemData>();

            List<DWDataTableManager.BoxData> newBoxDataList = new List<DWDataTableManager.BoxData>();
            InitBoxDatsLIst(ref newBoxDataList, (BOX_TYPE)p.boxType);
            double maxRate = 1.0;

            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            for (int i = 0; i < boxCount; ++i)
            {
                int boxNo = GetBoxNo(newBoxDataList, rand, maxRate);
                
                ulong rewardNo = newBoxDataList[boxNo].SerialNo;
                if(rewardNo == 0)
                {
                    continue;
                }

                maxRate -= newBoxDataList[boxNo].Rate;
                newBoxDataList.RemoveAt(boxNo);

                BoxDataTable boxDataTable = DWDataTableManager.GetDataTable(BoxDataTable_List.NAME, rewardNo) as BoxDataTable;
                if(boxDataTable == null)
                {
                    continue;
                }

                DWItemData itemData = new DWItemData();
                itemData.itemType = boxDataTable.ItemType;
                itemData.subType = boxDataTable.ItemSubType;
                itemData.value = boxDataTable.ItemValue;
                result.itemList.Add(itemData);

                DWMemberData.AddItem(itemData, ref gold, ref gem, ref cashGem, ref ether, ref cashEther, ref gas, ref cashGas, ref relicBoxCnt, ref skillItemList, ref boxList, ref droneAdvertisingOff, stageNo, logMessage);
            }

            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = string.Format("UPDATE DWMembersNew SET Gem = @gem, CashGem = @cashGem, Ether = @ether, CashEther = @cashEther, Gas = @gas, CashGas = @cashGas, SkillItemList = @skillItemList, BoxList=@boxList, RelicBoxCount=@relicBoxCount, DroneAdvertisingOff=@droneAdvertisingOff WHERE MemberID = '{0}'", p.memberID);
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
                    command.Parameters.Add("@droneAdvertisingOff", SqlDbType.Bit).Value = droneAdvertisingOff;

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
            result.droneAdvertisingOff = droneAdvertisingOff;
            result.errorCode = (byte)DW_ERROR_CODE.OK;

            return result;
        }

        void InitBoxDatsLIst(ref List<DWDataTableManager.BoxData> boxDataList, BOX_TYPE boxType)
        {
            List<DWDataTableManager.BoxData> originBoxDataList = DWDataTableManager.GetBoxDataList(boxType);
            
            for (int i = 0; i < originBoxDataList.Count; ++i)
            {
                DWDataTableManager.BoxData rewardBoxData = new DWDataTableManager.BoxData();
                rewardBoxData.Rate = originBoxDataList[i].Rate;
                rewardBoxData.SerialNo = originBoxDataList[i].SerialNo;
                boxDataList.Add(rewardBoxData);
            }

        }

        int GetBoxNo(List<DWDataTableManager.BoxData> boxDataList, Random rand, double maxRate)
        {
            double num = rand.NextDouble() * maxRate;
            for(int i = 0; i < boxDataList.Count; ++i)
            {
                num -= boxDataList[i].Rate;
                if(num <= 0.0)
                {
                    return i;
                }
            }

            return boxDataList.Count - 1;
        }
    }
}
