using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public static class SteamId
    {
        private const long MIN64_B = 76561197960265728;

        public static long ConvertTo64(long playerId)
        {
            if (playerId < MIN64_B)
                return playerId + MIN64_B;

            return playerId;
        }

        public static long ConvertTo32(long playerId)
        {
            if (playerId > MIN64_B)
                return playerId - MIN64_B;

            return playerId;
        }
    }
}
