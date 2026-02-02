using System.Runtime.InteropServices;

namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTime UtcToSutureDateTime(this DateTime dateTime)
        {
            var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            try
            {
                var databaseTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                                                               "Central Standard Time" :
                                                                               "America/Chicago");

                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, databaseTimeZone);
            }
            catch
            {
                return utcDateTime;
            }
        }

        public static DateTime SutureDateTimeToUtc(this DateTime dateTime)
        {
            var localDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);

            try
            {
                var databaseTimeZone = TimeZoneInfo.FindSystemTimeZoneById(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                                                               "Central Standard Time" :
                                                                               "America/Chicago");

                return TimeZoneInfo.ConvertTimeToUtc(localDateTime, databaseTimeZone);
            }
            catch
            {
                return localDateTime;
            }
        }

        public static DateTime UtcToSutureDateTime(this DateTimeOffset dateTimeOffset)
            => dateTimeOffset.UtcDateTime.UtcToSutureDateTime();

        public static bool Contains(this TimeSpan timespan, TimeSpan timeOfDay, byte gracePeriod)
            => timespan <= timeOfDay && timeOfDay <= (timespan + TimeSpan.FromMinutes(gracePeriod));
    }
}
