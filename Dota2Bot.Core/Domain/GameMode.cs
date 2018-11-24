using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public static class GameMode
    {
        public static List<string> Strings = new List<string>
        {
            "Unknown",
            "All Pick",
            "Captains Mode",
            "Random Draft",
            "Single Draft",
            "All Random",
            "Intro",
            "Diretide",
            "Reverse Captains Mode",
            "The Greeviling",
            "Tutorial",
            "Mid Only",
            "Least Played",
            "Limited Heroes",
            "Compendium",
            "Custom",
            "Captains Draft",
            "Balanced Draft",
            "Ability Draft",
            "Event",
            "All Random Deathmatch",
            "1v1 Solo Mid",
            "All Draft",
            "Turbo",
            "Mutation"
        };

        public static string Get(int gameMode)
        {
            if (gameMode < Strings.Count)
                return Strings[gameMode];

            return "Unknown GameMode";
        }
    }
}
