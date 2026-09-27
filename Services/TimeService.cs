namespace VolunteerHub.Services
{
    public class TimeService : ITimeService
    {
        public DateTime UtcNow =>
            DateTime.UtcNow;

        public DateTime GetCurrentLocalTime(
            string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                throw new ArgumentException(
                    "Time zone ID cannot be empty.",
                    nameof(timeZoneId));
            }

            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    timeZoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(
                UtcNow,
                timeZone);
        }
    }
}