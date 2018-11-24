using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public class RankTier
    {
        public static List<string> Strings = new List<string>
            {
                "Uncalibrated",
                "Herald",
                "Guardian",
                "Crusader",
                "Archon",
                "Legend",
                "Ancient",
                "Divine"
            };

        public static string Get(int? rankTier)
        {
            var rank = rankTier / 10 ?? 0;
            var tier = rankTier % 10 ?? 0;

            if (rank < Strings.Count)
            {
                var result = Strings[rank];

                if (tier > 0)
                {
                    result += $" [{tier}]";
                }

                return result;
            }
            
            return "Unknown RankTier";
        }
    }
}
