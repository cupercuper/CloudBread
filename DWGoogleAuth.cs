using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net;

using System.Linq;
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
using Newtonsoft.Json.Linq;
using CloudBreadAuth;
using System.Security.Claims;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using CloudBread.Models;
using System.IO;
using DW.CommonData;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Analytics.v3;



public class GoogleJsonWebToken
{
    public static GoogleJsonWebToken instance = new GoogleJsonWebToken();
    public string _token = "";
    public DateTime _expirationTime;

    public string Token
    {
        get
        {
            if(_token == "" || _expirationTime <= DateTime.Now)
            {
                _token = GoogleJsonWebToken.GetAccessToken();
                _expirationTime = DateTime.Now.AddMinutes(30);
            }

            return _token;
        }
    }

    public bool RequestVerifyFromGoogleStore(string productId, string purchasesToken, string packageName)
    {
        String URL = "https://www.googleapis.com/androidpublisher/v1.1/applications/" + packageName + "/inapp/" + productId + "/purchases/" + purchasesToken + "?access_token=" + Token;

        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
        req.Method = "GET";
        req.Accept = "application/json";
        WebResponse res = req.GetResponse();
        StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
        string result = reader.ReadToEnd();

        JObject obj = JObject.Parse(result);
        string state = obj["purchaseState"].ToString();

        if(state == "0")
        {
            return true;
        }

        return false;
    }

    public static string GetAccessToken()
    {
        byte[] readBytes = null;

        RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
        using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
        {
            string strQuery = string.Format("SELECT KeyFileByte FROM DWGoogleKeyFile WHERE [Index] = 1");
            using (SqlCommand command = new SqlCommand(strQuery, connection))
            {
                connection.OpenWithRetry(retryPolicy);
                using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
                {
                    while (dreader.Read())
                    {
                        readBytes = dreader[0] as byte[];
                    }
                }
            }
        }

        GoogleCredential credential;
        using (Stream stream = new MemoryStream(readBytes))
        {
            credential = GoogleCredential.FromStream(stream);
        }

        credential = credential.CreateScoped(new[]
        {
            "https://www.googleapis.com/auth/androidpublisher"
        });

        Task<string> task = ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
        task.Wait();
        string token = task.Result;

        return token;
    }
}


