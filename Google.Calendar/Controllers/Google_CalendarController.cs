using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Google.Calendar.Controllers
{
    public class Google_CalendarController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string[] Scopes = {
    "https://www.googleapis.com/auth/calendar",
    "https://www.googleapis.com/auth/calendar.events",
    "https://www.googleapis.com/auth/calendar.events.readonly",
    "https://www.googleapis.com/auth/calendar.readonly",
    "https://www.googleapis.com/auth/calendar.settings.readonly",
    "https://www.googleapis.com/auth/calendar.app.created",
    "https://www.googleapis.com/auth/calendar.calendars",
    "https://www.googleapis.com/auth/calendar.calendars.readonly",
    "https://www.googleapis.com/auth/calendar.acls",
    "https://www.googleapis.com/auth/calendar.acls.readonly",
    "https://www.googleapis.com/auth/calendar.events.owned",
    "https://www.googleapis.com/auth/calendar.events.owned.readonly",
    "https://www.googleapis.com/auth/calendar.events.public.readonly",
    "https://www.googleapis.com/auth/calendar.calendarlist",
    "https://www.googleapis.com/auth/calendar.calendarlist.readonly",
    "https://www.googleapis.com/auth/calendar.freebusy",
            CalendarService.Scope.Calendar,
    CalendarService.Scope.CalendarEvents,
    "https://www.googleapis.com/auth/meetings.space.created",
    "https://www.googleapis.com/auth/meetings.space.readonly"
};

        private static readonly string ApplicationName = "Web client 1";

        [HttpGet("")]
        public async Task<string> LoginWithGoogle()
        {
            var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = "1061994404638-5v9sp228ndn3h97paqi37h5jmaddsn9h.apps.googleusercontent.com";
            query["redirect_uri"] = "http://localhost:5179/auth/oauth2callback";
            query["scope"] = "email profile " + string.Join(" ", Scopes); // Boşluk ekledik
            query["response_type"] = "code";

            var uriBuilder = new UriBuilder("https://accounts.google.com/o/oauth2/auth");
            uriBuilder.Query = query.ToString();
            var authUrl = uriBuilder.Uri.ToString();
            return authUrl;
        }
        [HttpGet("/auth/oauth2callback")]
        public async Task<string> GetGoogleToken()
        {
            var queryString = HttpContext.Request.QueryString.Value;

            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);

            // Parametrelerin değerlerini al
            string code = queryParams["code"];
            return code;
            //return code;
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "1061994404638-5v9sp228ndn3h97paqi37h5jmaddsn9h.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
                Scopes = Scopes,
                DataStore = new FileDataStore("token.json")
            });

            var response = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:5179/auth/oauth2callback", CancellationToken.None);

            //return new UserCredential(flow, "user", response);
        }
        [HttpPost("/create-event")]
        public async Task<IActionResult> CreateEvent([FromBody] EventInfo eventInfo,string code)
        {
            try
            {

                // Parametrelerin değerlerini al
                // Google API'ya erişim için UserCredential nesnesini oluştur
                //var credential = await GetUserCredential(code);
                //return Ok(credential);
                UserCredential credential = await GetUserCredential(code);
                //return credential;
                //CalendarService nesnesini oluştur ve erişim yetkilendirmesini sağla
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var newEvent = new Event()
                {
                    Summary = eventInfo.Summary,
                    Location = eventInfo.Location,
                    Description = eventInfo.Description,
                    Start = new EventDateTime()
                    {
                        DateTime = eventInfo.StartDateTime,
                        TimeZone = eventInfo.TimeZone,
                    },
                    End = new EventDateTime()
                    {
                        DateTime = eventInfo.EndDateTime,
                        TimeZone = eventInfo.TimeZone,
                    },
                    Attendees = eventInfo.Attendees,
                    Reminders = new Event.RemindersData
                    {
                        UseDefault = false,
                        Overrides = new List<EventReminder>
        {
            new EventReminder { Method = "email", Minutes = 30 }
        }
                    },
                    ConferenceData = new ConferenceData()
                    {
                        CreateRequest = new CreateConferenceRequest()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey()
                            {
                                Type = "hangoutsMeet" // Google Meet konferansı kullanmak için
                            }
                        }
                    }
                };

                // Etkinliği oluştur
                EventsResource.InsertRequest request = service.Events.Insert(newEvent, "primary");
                Event createdEvent = request.Execute();
                return Ok(createdEvent.HangoutLink);


            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<UserCredential> GetUserCredential(string code)
        {
            UserCredential credential;
            GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = "1061994404638-5v9sp228ndn3h97paqi37h5jmaddsn9h.apps.googleusercontent.com",
                        ClientSecret = "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"
                    },
                    Scopes = Scopes,
                    DataStore = new FileDataStore("token.json")
                });

            //Uri authUri = flow.CreateAuthorizationCodeRequest("http://localhost:5179/auth/oauth2callback").Build();
            //Google.Apis.Auth.OAuth2.Responses.TokenResponse response = await flow.ExchangeCodeForTokenAsync(
            //    "user", code, "http://localhost:5179/auth/oauth2callback", CancellationToken.None);
            //return authUri;
            var response = await flow.ExchangeCodeForTokenAsync("user", code, "http://localhost:5179/auth/oauth2callback", CancellationToken.None);

            return new UserCredential(flow, "user", response);
        }
    }
}

public class EventInfo
{
    public string Summary { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string TimeZone { get; set; }
    public List<EventAttendee> Attendees { get; set; }
}
