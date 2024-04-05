using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZoomMeetingController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ZoomMeetingController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("/zoom/oauth2callback")]
        public async Task<ActionResult<string>> CreateMeeting()
        {
            string apiKey = "xzIJusiScaODLpuYR7Yzw";
            string apiSecret = "lJ40EFdkYi2CL8J3ZA2Pu6W6ltm24Yrv";
            string userEmail = "orxan_qanbarov@mail.ru";
            string meetingTopic = "Meeting Title";
            string startTime = "2024-04-06T12:00:00";
            int duration = 60; // Meeting duration in minutes

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.zoom.us/v2/");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GenerateJWT(apiKey, apiSecret));

            string requestBody = "{\"topic\": \"" + meetingTopic + "\", \"type\": 2, \"start_time\": \"" + startTime + "\", \"duration\": " + duration + ", \"timezone\": \"UTC\", \"settings\": {\"host_video\": false, \"participant_video\": false, \"audio\": \"both\"}}";

            HttpResponseMessage response = await client.PostAsync("users/" + userEmail + "/meetings", new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return Ok(responseData); // Return meeting link
            }
            else
            {
                return BadRequest("Error creating meeting: " + response.StatusCode);
            }
        }

        private string GenerateJWT(string apiKey, string apiSecret)
        {
            long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string payload = "{\"iss\": \"" + apiKey + "\", \"exp\": " + (unixTimestamp + 3600) + "}";
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            string base64Payload = Convert.ToBase64String(payloadBytes);

            using (System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
            {
                byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(base64Payload));
                string base64Signature = Convert.ToBase64String(signatureBytes);
                return base64Payload + "." + base64Signature;
            }
        }
    }
}
