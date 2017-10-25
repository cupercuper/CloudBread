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
            DWInsUserDataModel result = new DWInsUserDataModel()
            {
                MemberID = p.memberID,
                NickName = p.nickName,
                RecommenderID = p.recommenderID,
                CaptianLevel = 0,
                CaptianID = 0,
                LastWorld = 1,
                CurWorld = 1,
                CurStage = 1,
                UnitList = new Dictionary<uint, UnitData>(),
                CanBuyUnitList = DWDataTableManager.GetCanBuyUnitList(),
                Gold = 0,
                Gem = 0,
                EnhancedStone = 0
            };

            // Init Unit
            Dictionary<uint, UnitData> unitLIst = result.UnitList;
            DWMemberData.AddUnitDic(ref unitLIst, 1);
            DWMemberData.AddUnitDic(ref unitLIst, 2);
            DWMemberData.AddUnitDic(ref unitLIst, 3);
            //---------------------------------------------------------

            /// Database connection retry policy
            RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
            using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
            {
                string strQuery = "Insert (MemberID, NickName, RecommenderID, CaptianLevel, CaptianID, LastWorld, CurWorld, CurStage, UnitList, CanBuyUnitList, Gold, Gem, EnhancedStone) VALUES (@memberID, @nickName, @recommenderID, @captianLevel, @captianID, @lastWorld, @curWorld, @curStage, @unitList, @canBuyUnitList, @gold, @gem, @enhancedStone)";
                using (SqlCommand command = new SqlCommand(strQuery, connection))
                {
                    command.Parameters.Add("@memberID", SqlDbType.NVarChar).Value = result.MemberID;
                    command.Parameters.Add("@nickName", SqlDbType.NVarChar).Value = result.NickName;
                    command.Parameters.Add("@recommenderID", SqlDbType.NVarChar).Value = result.RecommenderID;
                    command.Parameters.Add("@captianLevel", SqlDbType.SmallInt).Value = result.CaptianLevel;
                    command.Parameters.Add("@captianID", SqlDbType.TinyInt).Value = result.CaptianID;
                    command.Parameters.Add("@lastWorld", SqlDbType.SmallInt).Value = result.LastWorld;
                    command.Parameters.Add("@curWorld", SqlDbType.SmallInt).Value = result.CurWorld;
                    command.Parameters.Add("@curStage", SqlDbType.SmallInt).Value = result.CurStage;
                    command.Parameters.Add("@unitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.UnitList);
                    command.Parameters.Add("@canBuyUnitList", SqlDbType.VarBinary).Value = DWMemberData.ConvertByte(result.CanBuyUnitList);
                    command.Parameters.Add("@gold", SqlDbType.Int).Value = result.Gold;
                    command.Parameters.Add("@gem", SqlDbType.Int).Value = result.Gem;
                    command.Parameters.Add("@enhancedStone", SqlDbType.Int).Value = result.EnhancedStone;

                    connection.OpenWithRetry(retryPolicy);

                    int rowCount = command.ExecuteNonQuery();
                    if (rowCount <= 0)
                    {
                        result.ErrorCode = (byte)DW_ERROR_CODE.OK;
                        return result;
                    }
                }
            }

            result.ErrorCode = (byte)DW_ERROR_CODE.OK;
            return result;
        }
    }
}
