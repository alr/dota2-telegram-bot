using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Domain.Entity
{
    [Table("players")]
    public class Player
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("steamid")]
        public string SteamId { get; set; }

        [Column("solo_rank")]
        public int SoloRank { get; set; }

        [Column("party_rank")]
        public int PartyRank { get; set; }

        [Column("win_count")]
        public int WinCount { get; set; }

        [Column("lose_count")]
        public int LoseCount { get; set; }

        [Column("last_match_id")]
        public long? LastMatchId { get; set; }

        [Column("last_match_date")]
        public DateTime? LastMatchDate { get; set; }

        [Column("last_rating_date")]
        public DateTime? LastRatingDate { get; set; }

        [Column("rank_tier")]
        public int? RankTier { get; set; }
        
        public virtual ICollection<Match> Matches { get; set; }

        public virtual ICollection<Rating> Ratings { get; set; }

        public virtual ICollection<TgChatPlayers> ChatPlayers { get; set; }

        public Player()
        {
            Matches = new HashSet<Match>();
            Ratings = new HashSet<Rating>();
            ChatPlayers = new HashSet<TgChatPlayers>();
        }

        public int TotalGames()
        {
            return WinCount + LoseCount;
        }

        public double WinRate()
        {
            var total = TotalGames();
            if (total == 0)
                return 0;

            var rate = WinCount*100d/total;

            return Math.Round(rate, 2);
        }
    }
}
