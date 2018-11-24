using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Domain.Entity
{
    [Table("matches")]
    public class Match
    {
        [Column("match_id", Order = 0)]
        public long MatchId { get; set; }

        [Column("player_id", Order = 1)]
        public long PlayerId { get; set; }

        [Column("player_slot")]
        public short PlayerSlot { get; set; }

        [Column("hero_id")]
        public int HeroId { get; set; }

        [Column("kills")]
        public int Kills { get; set; }

        [Column("deaths")]
        public int Deaths { get; set; }

        [Column("assists")]
        public int Assists { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Column("duration")]
        public TimeSpan Duration { get; set; }

        [Column("game_mode")]
        public int GameMode { get; set; }

        [Column("lobby_type")]
        public int LobbyType { get; set; }

        [Column("won")]
        public bool Won { get; set; }

        [Column("leaver_status")]
        public int LeaverStatus {get; set; }
        
        [ForeignKey("PlayerId")]
        public virtual Player Player { get; set; }

        [ForeignKey("HeroId")]
        public virtual Hero Hero { get; set; }
    }
}
