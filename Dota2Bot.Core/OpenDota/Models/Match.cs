using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.OpenDota.Models
{
    public class Match
    {
        public long account_id { get; set; }
        public long match_id { get; set; }
        public short player_slot { get; set; }
        public bool radiant_win { get; set; }
        public int duration { get; set; }
        public int game_mode { get; set; }
        public int lobby_type { get; set; }
        public int hero_id { get; set; }
        public long start_time { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int leaver_status { get; set; }
    }
}
