using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Domain
{
    public class LeaverStatus
    {
        public static List<string> Strings = new List<string>
        {
            "None",
            "Left Safely",
            "Abandoned (DC)",
            "Abandoned",
            "Abandoned (AFK)",
            "Never Connected",
            "Never Connected (Timeout)"
        };

        public static string Get(int leaverStatus)
        {
            if (leaverStatus < Strings.Count)
                return Strings[leaverStatus];

            return "Unknown LeaverStatus";
        }
    }
}
