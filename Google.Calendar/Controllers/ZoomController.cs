using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace YourNamespace.Controllers
{
    public class ZoomController : Controller
    {
        private readonly string _clientId = "rfHvKMNQRfK3PKI_uY4DQA";
        private readonly string _clientSecret = "dD72JZlbLmwmViL306f6nR4jg6tvSTLP";
        private readonly string _redirectUri = "https://localhost:7135/zoom/oauth2callback";
        private readonly string _accessToken = "eyJzdiI6IjAwMDAwMSIsImFsZyI6IkhTNTEyIiwidiI6IjIuMCIsImtpZCI6IjMyYTAzNDM2LWU2NGYtNDY3YS1hYjhjLTI5YTE4MGUxZjg3NSJ9.eyJ2ZXIiOjksImF1aWQiOiI2NzAyYmJhM2FlNzMwMWMyYjhiNWJlMDMwZGJkNGU1OSIsImNvZGUiOiJzbDZnaXVxVEJMOVdOWDN5MkYxUkh1a2Nob3VndDhPLVEiLCJpc3MiOiJ6bTpjaWQ6cmZIdktNTlFSZkszUEtJX3VZNERRQSIsImdubyI6MCwidHlwZSI6MCwidGlkIjowLCJhdWQiOiJodHRwczovL29hdXRoLnpvb20udXMiLCJ1aWQiOiJZem5WMTgzcVQ4U2FGZDZtd1kwNzNnIiwibmJmIjoxNzExNDg5NTcyLCJleHAiOjE3MTE0OTMxNzIsImlhdCI6MTcxMTQ4OTU3MiwiYWlkIjoiWXhZWjFiN2VSVEdEZEhqRFI4OXRxQSJ9.kv8BuRGMrQ5wciKfqxIu5rvnY7JFYuc9fhnO6vx_52q4ygCy2w_8UWM9FTWP7ZIhUgNnNPNHbZ10y8EJs4-rig";
        private readonly ITempDataDictionaryFactory _tempDataFactory; // ITempDataDictionaryFactory'yi tanımlayın

        public ZoomController(ITempDataDictionaryFactory tempDataFactory)
        {
            _tempDataFactory = tempDataFactory;
        }
        [HttpGet("/zoom")]
        public IActionResult AuthorizeZoom()
        {
            // Redirect to Zoom's authorization URL
            return Ok($"https://zoom.us/oauth/authorize?client_id={_clientId}&response_type=code&redirect_uri={_redirectUri}");
        }

        [HttpGet("/zoom/oauth2callback")]
        public async Task<IActionResult> OAuth2Callback(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                // If authorization code is not provided, return an error view or redirect to an error page
                return View("Error");
            }

            // Get access token using the provided authorization code
            var accessToken = await GetAccessToken(code);

            if (string.IsNullOrEmpty(accessToken))
            {
                // If access token retrieval fails, return an error view or redirect to an error page
                return View("Error");
            }

            // Access token retrieved successfully, do something with it
            return Ok(accessToken);
        }
        [HttpGet("/zoom/accesstoken")]
        private async Task<string> GetAccessToken(string authorizationCode)
        {
            using (var client = new HttpClient())
            {
                var tokenUrl = "https://zoom.us/oauth/token";
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUri)
                });

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}")));

                var response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }
                else
                {
                    return null;
                }
            }
        }
        [HttpGet("/zoom/create")]
        public async Task<IActionResult> CreateMeeting()
        {
            // Yeni toplantı oluşturmak için gerekli bilgiler
            var meetingDetails = new
            {
                topic = "My Zoom Meeting",
                type = 2, // 1: Instant Meeting, 2: Scheduled Meeting, 3: Recurring Meeting with no fixed time, 8: Recurring Meeting with fixed time
                start_time = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ"), // Toplantının başlangıç zamanı (ISO 8601 formatında)
                duration = 60, // Toplantının süresi (dakika cinsinden)
                timezone = "Europe/Istanbul" // Toplantının zaman dilimi
            };

            // Zoom API'si üzerindeki "Create a Meeting" endpoint'ine POST isteği gönderme
            var createdMeeting = await CreateMeeting(meetingDetails);

            if (createdMeeting != null)
            {
                ViewBag.MeetingID = createdMeeting.id;
                ViewBag.JoinURL = createdMeeting.join_url;
                return View();
            }
            else
            {
                return View("Error");
            }
        }

        private async Task<dynamic> CreateMeeting(object meetingDetails)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var apiUrl = "https://api.zoom.us/v2/users/me/meetings"; // Zoom API'sindeki "Create a Meeting" endpoint'inin URL'si

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(meetingDetails);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
