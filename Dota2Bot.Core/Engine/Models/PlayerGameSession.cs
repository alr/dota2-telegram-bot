using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.Engine.Models
{
    public class PlayerGameSession
    {
        public string SteamId { get; set; }

        public string PlayerName { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public int? Stat { get; set; }
    }
}
