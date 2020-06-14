using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Domain.Entity
{
    [Table("rating")]
    public class Rating
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("player_id")]
        public long PlayerId { get; set; }

        [Column("match_id")]
        public long MatchId { get; set; }

        [Column("solo_rank")]
        public int SoloRank { get; set; }

        [Column("party_rank")]
        public int PartyRank { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }
        
        [ForeignKey("PlayerId")]
        public virtual Player Player { get; set; }
    }
}

