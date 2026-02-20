using System;

namespace ITAMS.Utilities
{
    public static class DateTimeHelper
    {
        // Indian Standard Time (IST) is UTC+5:30
        private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        /// <summary>
        /// Gets the current date and time in Indian Standard Time (IST)
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstTimeZone);

        /// <summary>
        /// Converts UTC time to IST
        /// </summary>
        public static DateTime ToIst(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IstTimeZone);
        }

        /// <summary>
        /// Converts IST time to UTC
        /// </summary>
        public static DateTime ToUtc(DateTime istDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(istDateTime, IstTimeZone);
        }
    }
}
