using Google.Calendar.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Google.Calendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly IGoogleService _googleService;

        public GoogleController(IGoogleService googleService)
        {
            _googleService = googleService;
        }
        [HttpPost]
        public IActionResult CreateMeeting([FromBody] MeetingRequest request)
        {
            try
            {
                string result = _googleService.CreateMeeting(request.Email, request.Date, request.StartTime, request.EndTime, request.TimeZone);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
    public class MeetingRequest
    {
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TimeZone { get; set; }
    }
}
