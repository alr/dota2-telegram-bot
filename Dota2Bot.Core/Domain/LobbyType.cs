using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public static class LobbyType
    {
        public static List<string> Strings = new List<string>
        {
            "Normal",
            "Practice",
            "Tournament",
            "Tutorial",
            "Co-op with bots",
            "Team match",
            "Solo Queue",
            "Ranked",
            "Solo Mid 1vs1",
            "Battle Cup"
        };

        public static string Get(int lobbyType)
        {
            if (lobbyType < Strings.Count)
                return Strings[lobbyType];

            return "Unknown LobbyType";
        }
    }
}
