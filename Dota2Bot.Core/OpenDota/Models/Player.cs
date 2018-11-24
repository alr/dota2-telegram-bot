using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.OpenDota.Models
{
    public class MmrEstimate
    {
        public int estimate { get; set; }
        public double stdDev { get; set; }
        public int n { get; set; }
    }

    public class Profile
    {
        public long account_id { get; set; }
        public string personaname { get; set; }
        public string name { get; set; }
        public int cheese { get; set; }
        public string steamid { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string profileurl { get; set; }
        public object last_login { get; set; }
        public string loccountrycode { get; set; }
    }

    public class Player
    {
        public object tracked_until { get; set; }
        public int solo_competitive_rank { get; set; }
        public int competitive_rank { get; set; }
        public MmrEstimate mmr_estimate { get; set; }
        public Profile profile { get; set; }
        public int? rank_tier { get; set; }
    }
}
