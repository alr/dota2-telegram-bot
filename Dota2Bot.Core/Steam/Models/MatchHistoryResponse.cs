using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Dota2Bot.Core.SteamApi.Models
{
    public class MatchHistoryResponse
    {
        public class Player
        {
            public long account_id { get; set; }
            public int player_slot { get; set; }
            public int hero_id { get; set; }
        }

        public class Match
        {
            public long match_id { get; set; }
            public long match_seq_num { get; set; }
            public int start_time { get; set; }
            public int lobby_type { get; set; }
            public int radiant_team_id { get; set; }
            public int dire_team_id { get; set; }
            public List<Player> players { get; set; }

            public Match()
            {
                players = new List<Player>();
            }
        }

        public class Result
        {
            public int status { get; set; }
            public int num_results { get; set; }
            public int total_results { get; set; }
            public int results_remaining { get; set; }
            public List<Match> matches { get; set; }

            public Result()
            {
                matches = new List<Match>();
            }
        }

        public Result result { get; set; }
    }
}
