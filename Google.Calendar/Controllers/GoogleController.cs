using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Calendar.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Google.Calendar.Controllers
{
    public class GoogleController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpGet("")]
        public async Task<string> LoginWithGoogle()
        {
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = "1061994404638-5v9sp228ndn3h97paqi37h5jmaddsn9h.apps.googleusercontent.com";
            query["redirect_uri"] = "http://localhost:5179/auth/oauth2callback";
            query["scope"] = "email profile";
            query["response_type"] = "code";

            var uriBuilder = new UriBuilder("https://accounts.google.com/o/oauth2/auth");
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri.ToString();
            //return RedirectToAction(uriBuilder.Uri.ToString());
        }

        [HttpGet("/auth/oauth2callback")]
        public async Task<string> GetGoogleToken()
        {
            var queryString = HttpContext.Request.QueryString.Value;
        //http://localhost:5179/auth/oauth2callback
            // Query string içindeki parametreleri ayır ve oku
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);

            // Parametrelerin değerlerini al
            string code = queryParams["code"];
            return  code;
            string scope = queryParams["scope"];
            string authuser = queryParams["authuser"];
            string prompt = queryParams["prompt"];
            var postContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "1061994404638-5v9sp228ndn3h97paqi37h5jmaddsn9h.apps.googleusercontent.com"),
                new KeyValuePair<string, string>("client_secret", "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"),
                new KeyValuePair<string, string>("redirect_uri", "http://localhost:5179/auth/oauth2callback"),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
            });

            var response = await client.PostAsync("https://accounts.google.com/o/oauth2/token", postContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic tokenData = JsonConvert.DeserializeObject(responseContent);
            var token = tokenData.access_token;
            return token;
            //return await GetGoogleUserInfo(token);
        }

        private async Task<IActionResult> GetGoogleUserInfo(string token)
        {
            var response = await client.GetStringAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={Uri.EscapeDataString(token)}");
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(response);

            //var user = await GetUserByEmail(userInfo.email);
            //if (user == null)
            //{
               var user = new User
                {
                    Name = userInfo.given_name,
                    Email = userInfo.email,
                    Surname = userInfo.family_name,
                    EmailVerificationCode = string.Empty,
                    Status = 1
                };
                // Implement logic to save the new user to database
            //}

            // Implement logic to authenticate user
            // For example: await AuthenticateUserAsync(user);

            return Ok(user); // Replace this with your authentication logic
        }

        private async Task<User> GetUserByEmail(string email)
        {
            // Implement your logic to retrieve user by email
            return null; // Replace this with your actual logic
        }
    }

    //public class Config
    //{
    //    public class Services
    //    {
    //        public class Google
    //        {
    //            public static string ClientId { get; } = "YOUR_GOOGLE_CLIENT_ID";
    //            public static string ClientSecret { get; } = "YOUR_GOOGLE_CLIENT_SECRET";
    //            public static string RedirectUri { get; } = "YOUR_GOOGLE_REDIRECT_URI";
    //        }
    //    }
    //}
}
