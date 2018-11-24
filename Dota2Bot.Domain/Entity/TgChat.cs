using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Domain.Entity
{
    [Table("tg_chats")]
    public class TgChat
    {
        public TgChat()
        {
            Timezone = "Europe/Moscow";
            
            ChatPlayers = new HashSet<TgChatPlayers>();
        }

        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Column("timezone")]
        public string Timezone { get; set; }

        public virtual ICollection<TgChatPlayers> ChatPlayers { get; set; }
    }
}
