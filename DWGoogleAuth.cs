//using System;
//using System.Collections.Generic;
//using System.Text;
//using Newtonsoft.Json;
//using System.Net;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;


//using Google.Apis.AndroidPublisher.v2; 
//using Google.Apis.AndroidPublisher.v2.Data; 
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth.OAuth2.Flows; 
//using Google.Apis.Auth.OAuth2.Requests;  
//using Google.Apis.Auth.OAuth2.Responses;

//using Google.Apis.Services;

//using Google.Apis.Util.Store;

//namespace CloudBread
//{
//    //public class AuthResponse
//    //{
//    //    private string access_token;
//    //    public string Access_token
//    //    {
//    //        get
//    //        {
//    //            // Access token lasts an hour if its expired we get a new one.
//    //            if (DateTime.Now.Subtract(Created).Hours >= 1)
//    //            {
//    //                Refresh();
//    //            }
//    //            return access_token;
//    //        }
//    //        set { access_token = value; }
//    //    }
//    //    public string Refresh_token { get; set; }
//    //    public string ClientId { get; set; }
//    //    public string Secret { get; set; }
//    //    public string Expires_in { get; set; }
//    //    public DateTime Created { get; set; }

//    //    public static AuthResponse Instance = new AuthResponse();

//    //    //static string clientID = "717775335739-88g4ph5o0f11v1rgeaa5uekjdhq5dk61.apps.googleusercontent.com";
//    //    //static string clientPasword = "XYn - CB - lJYkiaFf9DUipeaH8";
//    //    //static string redirectUri = "http://dewartestmobile-ma.azurewebsites.net/api/DWGoogleAuth";

//    //    public static void TestAuth()
//    //    {

//    //        //D:\home\site\repository

//    //        //StringBuilder dataParam = new StringBuilder();
//    //        //dataParam.Append("scope=https://www.googleapis.com/auth/androidpublisher");
//    //        //dataParam.Append("&response_type=code");
//    //        //dataParam.Append("&access_type=offline");
//    //        //dataParam.Append("&redirect_uri=http://2344f833.ngrok.io/api/DWGoogleAuth");
//    //        //dataParam.Append("&client_id=717775335739-88g4ph5o0f11v1rgeaa5uekjdhq5dk61.apps.googleusercontent.com");

//    //        //byte[] byteDataParams = Encoding.ASCII.GetBytes(dataParam.ToString()); 

//    //        //string oauth = "https://accounts.google.com/o/oauth2/auth";

//    //        //string uristring = string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&scope=https://www.googleapis.com/auth/androidpublisher&response_type=code&access_type=offline", "717775335739-88g4ph5o0f11v1rgeaa5uekjdhq5dk61.apps.googleusercontent.com", "http://2344f833.ngrok.io/api/DWGoogleAuth");
//    //        //Uri uri = new Uri(uristring);
//    //        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth);
//    //        //request.Method = "POST";
//    //        //request.ContentType = "application/x-www-form-urlencoded";
//    //        //request.ContentLength = byteDataParams.Length;

//    //        //using (var stream = request.GetRequestStream())
//    //        //{
//    //        //    stream.Write(byteDataParams, 0, byteDataParams.Length);
//    //        //}

//    //        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//    //        //// 응답 Stream 읽기
//    //        //string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
//    //        ////Stream stReadData = response.GetResponseStream();
//    //        ////StreamReader srReadData = new StreamReader(stReadData, Encoding.Default);

//    //        ////// 응답 Stream -> 응답 String 변환
//    //        ////string strResult = srReadData.ReadToEnd();

//    //        ////StringBuilder dataParam = new StringBuilder();
//    //        ////dataParam.Append("scope=https://www.googleapis.com/auth/androidpublisher");
//    //        ////dataParam.Append("&response_type=code");
//    //        ////dataParam.Append("&access_type=offline");
//    //        ////dataParam.Append("&redirect_uri=http://dewartestmobile-ma.azurewebsites.net/api/DWGoogleAuth");
//    //        ////dataParam.Append("&client_id=717775335739-88g4ph5o0f11v1rgeaa5uekjdhq5dk61.apps.googleusercontent.com");

//    //        ////byte[] byteDataParams = UTF8Encoding.UTF8.GetBytes(dataParam.ToString());

//    //        ////string oauth  = "https://accounts.google.com/o/oauth2/auth";

//    //        string uristring = string.Format("https://accounts.google.com/o/oauth2/auth?client_id=717775335739-1dl9nv860idkollth4r0sba4n2smt4jl.apps.googleusercontent.com&redirect_uri=urn:ietf:wg:oauth:2.0:oob&scope=https://www.googleapis.com/auth/androidpublisher&response_type=code&access_type=offline");
//    //        Uri uri = new Uri(uristring);
//    //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
//    //        request.Method = "GET";
//    //        //request.ContentType = "application/x-www-form-urlencoded";
//    //        //request.ContentLength = byteDataParams.Length;

//    //        //Stream stDataParams = request.GetRequestStream();
//    //        //stDataParams.Write(byteDataParams, 0, byteDataParams.Length);
//    //        //stDataParams.Close();

//    //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//    //        // 응답 Stream 읽기
//    //        Stream stReadData = response.GetResponseStream();
//    //        StreamReader srReadData = new StreamReader(stReadData, Encoding.Default);

//    //        // 응답 Stream -> 응답 String 변환
//    //        string strResult = srReadData.ReadToEnd();


//    //        //    //Google.Apis.Auth.OAuth2.ServiceCredential
//    //        //    ClientSecrets clientSecrets = new ClientSecrets
//    //        //    {
//    //        //        ClientId = clientID, 
//    //        //        ClientSecret = clientPasword
//    //        //    };


//    //        //    //AndroidPublisherService service = new AndroidPublisherService(AndroidPublisherService.Initializer());
//    //        //    //GoogleAuthorizationCodeFlow credential = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//    //        //    //{
//    //        //    //    ClientSecrets = clientSecrets,
//    //        //    //    Scopes = new[] { AndroidPublisherService.Scope.Androidpublisher}
//    //        //    //});



//    //        //    //AuthorizationCodeRequestUrl url = credential.CreateAuthorizationCodeRequest(redirectUri);

//    //        //    ////string result = url.Build().ToString();
//    //        //    ////result = "";
//    //        //    //return new RedirectResult(url.Build().ToString());

//    //        //    //GoogleWebAuthorizationBroker.AuthorizeAsync()


//    //        //ServiceAccountCredential credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("contact@mons-ent.com")
//    //        //    {
//    //        //        Scopes = new[] { "https://www.googleapis.com/auth/androidpublisher" }
//    //        //    });

//    //        //    var request = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/auth/androidpublisher");

//    //        //    string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code", authCode, clientid, secret, redirectURI);
//    //        //    var data = Encoding.ASCII.GetBytes(postData);

//    //        //    request.Method = "POST";
//    //        //    request.ContentType = "application/x-www-form-urlencoded";
//    //        //    request.ContentLength = data.Length;

//    //        //    using (var stream = request.GetRequestStream())
//    //        //    {
//    //        //        stream.Write(data, 0, data.Length);
//    //        //    }

//    //        //    var response = (HttpWebResponse)request.GetResponse();

//    //        //    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

//    //        //    var x = AuthResponse.Get(responseString);

//    //        //    x.ClientId = clientid;
//    //        //    x.Secret = secret;

//    //    }

//    //    /// 
//    //    /// Parse the json response 
//    //    /// //  "{\n  \"access_token\" : \"ya29.kwFUj-la2lATSkrqFlJXBqQjCIZiTg51GYpKt8Me8AJO5JWf0Sx6-0ZWmTpxJjrBrxNS_JzVw969LA\",\n  \"token_type\" : \"Bearer\",\n  \"expires_in\" : 3600,\n  \"refresh_token\" : \"1/ejoPJIyBAhPHRXQ7pHLxJX2VfDBRz29hqS_i5DuC1cQ\"\n}"
//    //    /// 
//    //    /// 
//    //    /// 
//    //    public static AuthResponse Get(string response)
//    //    {
//    //        AuthResponse result = JsonConvert.DeserializeObject(response) as AuthResponse;
//    //        result.Created = DateTime.Now;   // DateTime.Now.Add(new TimeSpan(-2, 0, 0)); //For testing force refresh.
//    //        return result;
//    //    }


//    //    public void Refresh()
//    //    {
//    //        var request = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
//    //        string postData = string.Format("client_id={0}&client_secret={1}&refresh_token={2}&grant_type=refresh_token", this.ClientId, this.Secret, this.Refresh_token);
//    //        var data = Encoding.ASCII.GetBytes(postData);

//    //        request.Method = "POST";
//    //        request.ContentType = "application/x-www-form-urlencoded";
//    //        request.ContentLength = data.Length;

//    //        using (var stream = request.GetRequestStream())
//    //        {
//    //            stream.Write(data, 0, data.Length);
//    //        }

//    //        var response = (HttpWebResponse)request.GetResponse();
//    //        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
//    //        var refreshResponse = AuthResponse.Get(responseString);
//    //        this.access_token = refreshResponse.access_token;
//    //        this.Created = DateTime.Now;
//    //    }


//    //    public static AuthResponse Exchange(string authCode, string clientid, string secret, string redirectURI)
//    //    {
//    //        //AndroidPublisherService(new AndroidPublisherService.Initializer { });
//    //        var request = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");

//    //        string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code", authCode, clientid, secret, redirectURI);
//    //        var data = Encoding.ASCII.GetBytes(postData);

//    //        request.Method = "POST";
//    //        request.ContentType = "application/x-www-form-urlencoded";
//    //        request.ContentLength = data.Length;

//    //        using (var stream = request.GetRequestStream())
//    //        {
//    //            stream.Write(data, 0, data.Length);
//    //        }

//    //        var response = (HttpWebResponse)request.GetResponse();

//    //        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

//    //        var x = AuthResponse.Get(responseString);

//    //        x.ClientId = clientid;
//    //        x.Secret = secret;

//    //        return x;

//    //    }
//    //}
//}

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
using Newtonsoft.Json;
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
    //private static ILogger Log = LogManager.GetCurrentClassLogger();
    public const string SCOPE_AUTH_ANDROIDPUBLISHER = "https://www.googleapis.com/auth/androidpublisher";

    //public static dynamic GetAccessToken(string clientIdEMail, string keyFilePath, string szScope)
    //{
        public static dynamic GetAccessToken(string clientIdEMail, string keyFilePath, string scope)
        {

        //byte[] rawData = null;
        //RetryPolicy retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(globalVal.conRetryCount, TimeSpan.FromSeconds(globalVal.conRetryFromSeconds));
        //using (SqlConnection connection = new SqlConnection(globalVal.DBConnectionString))
        //{
        //    string strQuery = string.Format("SELECT KeyFileByte FROM DWGoogleKeyFile WHERE [Index] = 1");
        //    using (SqlCommand command = new SqlCommand(strQuery, connection))
        //    {
        //        connection.OpenWithRetry(retryPolicy);
        //        using (SqlDataReader dreader = command.ExecuteReaderWithRetry(retryPolicy))
        //        {
        //            while (dreader.Read())
        //            {
        //                rawData = dreader[0] as byte[];
        //            }
        //        }
        //    }
        //}
        //// certificate
        ////var certificate = new X509Certificate2(keyFilePath, "notasecret");
        //var certificate = new X509Certificate2(rawData, "notasecret");

        //// header
        //var header = new { typ = "JWT", alg = "RS256" };

        //    // claimset
        //    var times = GetExpiryAndIssueDate();
        //    var claimset = new
        //    {
        //        iss = clientIdEMail,
        //        scope = scope,
        //        aud = "https://accounts.google.com/o/oauth2/token",
        //        iat = times[0],
        //        exp = times[1],
        //    };

        //    JavaScriptSerializer ser = new JavaScriptSerializer();

        //// encoded header
        //var headerSerialized = ser.Serialize(header);
        //var headerBytes = Encoding.UTF8.GetBytes(headerSerialized);
        //var headerEncoded = Convert.ToBase64String(headerBytes);

        //// encoded claimset
        //var claimsetSerialized = ser.Serialize(claimset);
        //var claimsetBytes = Encoding.UTF8.GetBytes(claimsetSerialized);
        //var claimsetEncoded = Convert.ToBase64String(claimsetBytes);

        //// input
        //var input = headerEncoded + "." + claimsetEncoded;
        //var inputBytes = Encoding.UTF8.GetBytes(input);

        //// signiture
        //var rsa = certificate.PrivateKey as RSACryptoServiceProvider;
        ////var cspParam = new CspParameters
        ////{
        //    //KeyContainerName = rsa.CspKeyContainerInfo.KeyContainerName,
        //    //KeyNumber = rsa.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2
        ////};
        ////var aescsp = new RSACryptoServiceProvider(cspParam) { PersistKeyInCsp = false };
        ////var signatureBytes = aescsp.SignData(inputBytes, "SHA256");
        ////var signatureEncoded = Convert.ToBase64String(signatureBytes);

        ////// jwt
        ////var jwt = headerEncoded + "." + claimsetEncoded + "." + signatureEncoded;

        ////var client = new WebClient();
        ////client.Encoding = Encoding.UTF8;
        ////var uri = "https://accounts.google.com/o/oauth2/token";
        ////var content = new NameValueCollection();

        ////content["assertion"] = jwt;
        ////content["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        //// string response = Encoding.UTF8.GetString(client.UploadValues(uri, "POST", content));

        ////var result = ser.Deserialize<dynamic>(response);

        ////return result;
        //return null;


        //// Get active credential
        //string credPath = "F:\\StarHeroesDefence-8de7e9dc4a7c.json";
        
        //var json = File.ReadAllText(credPath);
        ////var cr = JsonConvert.DeserializeObject<PersonalServiceAccountCred>(json); // "personal" service account credential
        //Newtonsoft.Json.Linq.JObject cr = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);

        //// Create an explicit ServiceAccountCredential credential
        ////var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(cr.ClientEmail)
        ////{
        ////    Scopes = new[] {
        ////    AnalyticsService.Scope.AnalyticsManageUsersReadonly,
        ////    AnalyticsService.Scope.AnalyticsReadonly
        ////}
        ////}.FromPrivateKey(cr.PrivateKey));

        //string clientEmail = "mong-ent@starheroesdefence-3072171.iam.gserviceaccount.com";
        //string privateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDG3bX/m1ddDOlQ\n8Ek9pHi8DPKmtldWScgh/pcZC4lOgcAy8Q2oq4SMoQ5EgaAmMBx9SVV+owo10x5y\nFbv/zFHo5S65cPIYjah1eMm3nLPrmgPEEf+/L+hh1cs8Z7WX1IGTyIWBxFC0dSpm\nqgyc+547QdOkDu66ycXPG0uPPAHwMUJAM5oVJZDITG6oh8iUBFUHXc1QbwD8FHA3\nWpEBflJRTTypt2fSAmn9B42dpezlHQhjPVFfqtPzW1sIMMUIh7CXnZMZEHFjkBqY\nqC332woEfv1JzxlQJvv5kCnYKkZnrH3RZmfr2Ty3zZsqDu2fLyuCK7a81NWiW5hr\nb8KPw1URAgMBAAECggEABIy6PxmzWRtEY8AJlu3SPtAnhYmmrCBWQjPYSmAFkUyd\n5HR4IOeiqdJ9+HKEQtdnMXUZ7gKmJ0bYG5lUcqTeq17B78duHIJyLBTaPZeJ1L4y\nb5wPTzUWApYA9zXytUFtAP/tkQNYKtbC5HQnw3gsvbOS914Jl2+qFUPNOd7QNwnp\nIUb5NH2xRU6AkgfVjS9V5ofoRv2OqG2NMAC9s9/b77e6kLF+e2VKPxCV+euKzCSy\nc1FeBToZlLblkKP+ZePCiK+Ymyg2oGgrfL773KtPVUEbHz6hDoasZ3FBosjv6wDU\nqDHTb9DUR1ylsES4z8f0TfE75S9yOsFVeMV2Phy8gQKBgQDs/2/NPeSALAofjjt+\noSorfCFdDtzVHB2NMOhGeAxQr6r21btxF9Dlxrs2yPDwSKofNOX9fjpxZHQGOjyW\n4K+LlUCtAbcO26JF1BowvBctRF3NQN4vNYx3ekXFBNPtfHvKaSV/OTLkfZoWFRzf\nIFX8p5JJBkAinhH+pUIA8xBVUQKBgQDWz5c4Il/83nVsW3jb10EwHmb2r9ppLBlb\nLDETsGJlErK41VGUh3sEJA7S6eAVDxmpumCTOsdCYm4lJBS979C7AvsfdWR6IuMS\nROPv7rYuevLVA7ZHUlTeVAYbUNwba6uwpldLvojgUMBTxjq2lhvR+AYRH4FFY9jf\n/lHzSL4TwQKBgGNTzzy6wujBDGEbUG4LB8x/0s/Yqc8bYHtLUx4ttCOg7EtVWqDU\ndXXH67DTWL+sBLjdh8LNQ5tAmav+fmRRuOEMZbXcsScmAAW6Flv0iNYtAA9FL5mj\nnMJ6WYFwO44eDRfRJ6kBhQXkak70/Eu7lq40YQf/+aNIkowiIN6bAG5xAoGAMy5t\nClvuYPKkKbg1uLO1/YAQ6lAGCy8Nr5J88wMAvrJQ0QdKk8ggAGGTAp/k2z+ozq7w\nmcPUfDW1prXNF2BPbG+OY/V1hkBPuCef46gN0k9LEoP6501vmTWN6u4+M25ZDxLj\nE9IHDONQn+VIUf68DDA7SdMm2uCqaNxQrFW1rEECgYEAoGOXF8yKHra7JkdHhwR7\nqv/m/RTG2tk8Os4VZas0oatQcOLkKtSGzN9e/Ia5xXgVP70GWsPNTqixgN5uwJqq\nKwS/1yAWDqCbTJmrJmS+hr7n+6CS/m72pNaZsbS91GwRiH7ZXnCrc0xTt85P2Vw2\nhvbz01SwkAWR6gEvzUr3e24=\n-----END PRIVATE KEY-----\n";
        //var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(clientEmail)
        //{
        //    Scopes = new[] {
        //    "https://www.googleapis.com/auth/androidpublisher"
        //}
        //}.FromPrivateKey(privateKey));

        //// Create the service
        //AnalyticsService service = new AnalyticsService(
        //    new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = xCred,
        //    }
        //);

        //// some calls to Google API
        //var act1 = service.Management.Accounts.List().Execute();

        //var actSum = service.Management.AccountSummaries.List().Execute();

        //var resp1 = service.Management.Profiles.List(actSum.Items[0].Id, actSum.Items[0].WebProperties[0].Id).Execute();

        //return resp1;

        GoogleCredential credential;

        string credPath = "D:\\home\\site\\repository\\StarHeroesDefence-8de7e9dc4a7c.json";
        using (Stream stream = new FileStream(credPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            credential = GoogleCredential.FromStream(stream);
        }
        credential = credential.CreateScoped(new[] {
       "https://www.googleapis.com/auth/androidpublisher" });

        Task<string> task = ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
        task.Wait();
        string token = task.Result;

        return token;
    }
    //}





    private static string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0]; // Remove any trailing '='s
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }



    private static int[] GetExpiryAndIssueDate()
    {
        var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var issueTime = DateTime.UtcNow;

        var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
        var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;

        return new[] { iat, exp };
    }
}


