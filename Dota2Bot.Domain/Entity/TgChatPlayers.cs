using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Dota2Bot.Domain.Entity
{
    [Table("tg_chat_players")]
    public class TgChatPlayers
    {
        [Column("id_chat")]
        public long ChatId { get; set; }

        [Column("id_player")]
        public long PlayerId { get; set; }

        [ForeignKey("ChatId")]
        public virtual TgChat Chat { get; set; }

        [ForeignKey("PlayerId")]
        public virtual Player Player { get; set; }
    }
}
