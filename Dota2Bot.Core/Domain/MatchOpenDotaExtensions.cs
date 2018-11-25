using System;
using System.Collections.Generic;
using System.Text;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.OpenDota.Models;

namespace Dota2Bot.Core.Domain
{
    public static class MatchOpenDotaExtensions
    {
        public static bool Won(this Match match)
        {
            var isRadiant = PlayerSlot.IsRadiant(match.player_slot);
            return isRadiant == match.radiant_win;
        }

        public static DateTime DateStart(this Match match)
        {
            return DateTimeHelper.UnixTimeStampToDateTime(match.start_time);
        }

        public static DateTime DateEnd(this Match match)
        {
            return DateTimeHelper.UnixTimeStampToDateTime(match.start_time + match.duration);
        }

        public static TimeSpan Duration(this Match match)
        {
            return TimeSpan.FromSeconds(match.duration);
        }
    }
}
