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

        bool re = bool.Parse(result);
        return true;
    }

    public static string GetAccessToken()
    {
        GoogleCredential credential;

        string credPath = "D:\\home\\site\\repository\\StarHeroesDefence-8de7e9dc4a7c.json";
        using (Stream stream = new FileStream(credPath, FileMode.Open, FileAccess.Read, FileShare.Read))
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


