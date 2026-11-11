using System;

namespace Joja.Api.Services
{
    public static class TimeHelper
    {
        private static readonly Lazy<TimeZoneInfo> _egyptTimeZone = new Lazy<TimeZoneInfo>(() =>
        {
            try
            {
                // Linux / Docker / Render (IANA ID)
                return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
            }
            catch
            {
                try
                {
                    // Windows (Windows ID)
                    return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                }
                catch
                {
                    // Fallback fixed UTC+2
                    return TimeZoneInfo.CreateCustomTimeZone("EgyptStandardTimeFallback", TimeSpan.FromHours(2), "Cairo Time", "Cairo Standard Time");
                }
            }
        });

        public static TimeZoneInfo EgyptTimeZone => _egyptTimeZone.Value;

        /// <summary>
        /// Returns the current local DateTime in Egypt.
        /// </summary>
        public static DateTime GetEgyptNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTimeZone);
        }

        /// <summary>
        /// Converts an input DateTime (from local Egypt admin input) to UTC for safe storage.
        /// </summary>
        public static DateTime? ToUtcFromEgypt(DateTime? localDateTime)
        {
            if (!localDateTime.HasValue) return null;
            var dt = localDateTime.Value;
            var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, EgyptTimeZone);
        }

        /// <summary>
        /// Converts stored UTC DateTime to Egypt local time for display/editing.
        /// </summary>
        public static DateTime? ToEgyptFromUtc(DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue) return null;
            var dt = utcDateTime.Value;
            var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, EgyptTimeZone);
        }
    }
}
