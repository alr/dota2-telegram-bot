﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Extensions;

// ReSharper disable InconsistentNaming

namespace Dota2Bot.Core.OpenDota.Models
{
    public class MatchDetailsResponse
    {
        public Result result { get; set; }

        public class AbilityUpgrade
        {
            public int ability { get; set; }
            public int time { get; set; }
            public int level { get; set; }
        }

        public class Player
        {
            public long account_id { get; set; }
            public short player_slot { get; set; }
            public int hero_id { get; set; }
            public int item_0 { get; set; }
            public int item_1 { get; set; }
            public int item_2 { get; set; }
            public int item_3 { get; set; }
            public int item_4 { get; set; }
            public int item_5 { get; set; }
            public int backpack_0 { get; set; }
            public int backpack_1 { get; set; }
            public int backpack_2 { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public int assists { get; set; }
            public int leaver_status { get; set; }
            public int last_hits { get; set; }
            public int denies { get; set; }
            public int gold_per_min { get; set; }
            public int xp_per_min { get; set; }
            public int level { get; set; }
            public int hero_damage { get; set; }
            public int tower_damage { get; set; }
            public int hero_healing { get; set; }
            public int gold { get; set; }
            public int gold_spent { get; set; }
            public List<AbilityUpgrade> ability_upgrades { get; set; }

            public Player()
            {
                ability_upgrades = new List<AbilityUpgrade>();
            }
        }

        public class PicksBan
        {
            public bool is_pick { get; set; }
            public int hero_id { get; set; }
            public int team { get; set; }
            public int order { get; set; }
        }

        public class Result
        {
            public List<Player> players { get; set; }
            public bool radiant_win { get; set; }
            public int duration { get; set; }
            public int start_time { get; set; }
            public long match_id { get; set; }
            public long match_seq_num { get; set; }
            public int tower_status_radiant { get; set; }
            public int tower_status_dire { get; set; }
            public int barracks_status_radiant { get; set; }
            public int barracks_status_dire { get; set; }
            public int cluster { get; set; }
            public int first_blood_time { get; set; }
            public int lobby_type { get; set; }
            public int human_players { get; set; }
            public int leagueid { get; set; }
            public int positive_votes { get; set; }
            public int negative_votes { get; set; }
            public int game_mode { get; set; }
            public int engine { get; set; }
            public int radiant_score { get; set; }
            public int dire_score { get; set; }
            public List<PicksBan> picks_bans { get; set; }

            public Result()
            {
                picks_bans = new List<PicksBan>();
                players = new List<Player>();
            }

            #region Public

            public DateTime DateStart()
            {
                return DateTimeHelper.UnixTimeStampToDateTime(this.start_time);
            }

            public DateTime DateEnd()
            {
                return DateTimeHelper.UnixTimeStampToDateTime(this.start_time + this.duration);
            }

            public TimeSpan Duration()
            {
                return TimeSpan.FromSeconds(this.duration);
            }

            public bool Won(Player player)
            {
                var isRadiant = PlayerSlot.IsRadiant(player.player_slot);
                return isRadiant == this.radiant_win;
            }

            #endregion
        }
    }
}
