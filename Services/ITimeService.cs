namespace VolunteerHub.Services
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }

        DateTime GetCurrentLocalTime(string timeZoneId);
    }
}