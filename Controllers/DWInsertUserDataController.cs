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
    public class DWInsertUserDataController : ApiController
    {
        // GET api/DWInsertUserData
        public string Get()
        {
            return "Hello from custom controller!";
        }

        public HttpResponseMessage Post(DWInsUserDataInputParams p)
        {
            // try decrypt data
            if (!string.IsNullOrEmpty(p.token) && globalVal.CloudBreadCryptSetting == "AES256")
            {
                try
                {
                    string decrypted = Crypto.AES_decrypt(p.token, globalVal.CloudBreadCryptKey, globalVal.CloudBreadCryptIV);
                    p = JsonConvert.DeserializeObject<DWInsUserDataInputParams>(decrypted);

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
                DWInsUserDataModel result = GetResult(p);

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
                logMessage.Logger = "DWInsertUserDataController";
                logMessage.Message = jsonParam;
                logMessage.Exception = ex.ToString();
                Logging.RunLog(logMessage);

                throw;
            }
        }

        DWInsUserDataModel GetResult(DWInsUserDataInputParams p)
        {
            DWInsUserDataModel result = new DWInsUserDataModel();
            result.userData = new DWUserData()
            {
                memberID = p.memberID,
                nickName = p.nickName,
                recommenderID = p.recommenderID,
                captianLevel = 0,
                captianID = 0,
                lastWorld = 1,
                curWorld = 1,
                curStage = 1,
                unitList = null,
                canBuyUnitList = DWDataTableManager.GetCanBuyUnitList(),
                gold = 0,
                gem = 0,
                enhancedStone = 0
            };

            // Init Unit
            Dictionary<uint, UnitData> unitDic = new Dictionary<uint, UnitData>();
            DWMemberData.AddUnitDic(ref unitDic, 1);
            DWMemberData.AddUnitDic(ref unitDic, 2);
            DWMemberData.AddUnitDic(ref unitDic, 3);
            result.userData.unitList = DWMemberData.ConvertClientUnitData(unitDic);
            //---------------------------------------------------------

            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "Insert into DWMembers (MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, LastWorld, CurWorld, CurStage, UnitList, CanBuyUnitList, Gold, Gem, EnhancedStone) VALUES (@memberID, @nickName, @recommenderID, @captianLevel, @captianID, @lastWorld, @curWorld, @curStage, @unitList, @canBuyUnitList, @gold, @gem, @enhancedStone)";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = result.userData.memberID;
                    command.Parameters.Add("@nickName", SqlDbType.NVarChar).Value = result.userData.nickName;
                    command.Parameters.Add("@recommenderID", SqlDbType.NVarChar).Value = result.userData.recommenderID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = result.userData.captianLevel;
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = result.userData.captianID;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = result.userData.lastWorld;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = result.userData.curWorld;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = result.userData.curStage;
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(unitDic);
                    command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.userData.canBuyUnitList);
                    command.Parameters.Add("@gold", SqlDbType.Int).Value = result.userData.gold;
                    command.Parameters.Add("@gem", SqlDbType.Int).Value = result.userData.gem;
                    command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = result.userData.enhancedStone;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.errorCode = (byte)DW_ERROR_CODE.OK;
                        return result;
                    }
                }
            }

            result.errorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
