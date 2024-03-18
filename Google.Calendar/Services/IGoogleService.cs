namespace Google.Calendar.Services
{
    public interface IGoogleService
    {
        string CreateMeeting(string email, DateTime date, string startTime, string endTime, string timeZone);
    }
}
