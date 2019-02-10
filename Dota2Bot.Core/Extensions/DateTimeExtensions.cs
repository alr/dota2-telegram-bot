using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace Dota2Bot.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            var a = dt.AddDays(-1 * diff).Date;
            return new DateTime(a.Year, a.Month, a.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime StartOfYear(this DateTime dt)
        {
            return new DateTime(dt.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime ConvertToTimezone(this DateTime dt, string timezone)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            if (dt.Kind == DateTimeKind.Local)
                dt = dt.ToUniversalTime();

            var tzdb = DateTimeZoneProviders.Tzdb;
            var timezone2 = timezone != null
                ? tzdb.GetZoneOrNull(timezone) ?? tzdb.GetSystemDefault()
                : tzdb.GetSystemDefault();

            return Instant.FromDateTimeUtc(dt).InZone(timezone2).ToDateTimeUnspecified();
        }

        public static DateTime ConvertFromTimezone(this DateTime dt, string timezone)
        {
            var tzdb = DateTimeZoneProviders.Tzdb;
            var timezone2 = timezone != null
                ? tzdb.GetZoneOrNull(timezone) ?? tzdb.GetSystemDefault()
                : tzdb.GetSystemDefault();

            var local = LocalDateTime.FromDateTime(dt);
            var zoned = timezone2.AtStrictly(local);
            return zoned.ToDateTimeUtc();
        }
    }
}
