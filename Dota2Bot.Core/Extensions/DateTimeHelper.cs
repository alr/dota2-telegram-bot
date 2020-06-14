using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2Bot.Core.Extensions
{
    public static class DateTimeHelper
    {
        private static DateTime _unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return _unixStartDate.AddSeconds(unixTimeStamp);
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long) ((dateTime - _unixStartDate).TotalSeconds);
        }
    }
}
