using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;

namespace Google.Calendar.Services
{
    public class GoogleService : IGoogleService
    {
        private static readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private static readonly string ApplicationName = "Google Calendar API C# Meet Creation";

        public string CreateMeeting(string email, DateTime date, string startTime, string endTime, string timeZone)
        {
            UserCredential credential;

            List<string> redirectUris = new List<string>
            {
                "http://localhost:7135" // Your redirect URI here
            };

            using (var stream = new FileStream("cre.json", FileMode.Open, FileAccess.Read))
            {
                // Load client secrets from JSON file
                ClientSecrets clientSecrets = GoogleClientSecrets.Load(stream).Secrets;

                // Create new instance of AuthorizationCodeFlow
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes,
                    DataStore = new FileDataStore("token.json"),
                });

                // Update redirect URIs
                var tokenRequest = flow.CreateAuthorizationCodeRequest(redirectUris[0]);

                // Authorization URL
                var authorizationUrl = tokenRequest.Build();

                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define the event
            Event newEvent = new Event()
            {
                Summary = "Meeting",
                Location = "Online", // Google Meet
                Description = "A test meeting created with Google Calendar API",
                Start = new EventDateTime()
                {
                    DateTime = new DateTime(date.Year, date.Month, date.Day, int.Parse(startTime.Substring(0, 2)), int.Parse(startTime.Substring(3, 2)), 0),
                    TimeZone = timeZone,
                },
                End = new EventDateTime()
                {
                    DateTime = new DateTime(date.Year, date.Month, date.Day, int.Parse(endTime.Substring(0, 2)), int.Parse(endTime.Substring(3, 2)), 0),
                    TimeZone = timeZone,
                },
                Attendees = new List<EventAttendee>()
                {
                    new EventAttendee() { Email = email }
                },
                ConferenceData = new ConferenceData()
                {
                    CreateRequest = new CreateConferenceRequest()
                    {
                        RequestId = Guid.NewGuid().ToString() // Provide a unique ID
                    }
                }
            };

            // Insert the event
            EventsResource.InsertRequest request = service.Events.Insert(newEvent, "primary");
            Event createdEvent = request.Execute();
            return "Event created: " + createdEvent.HtmlLink;
        }
    }
}
