using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dota2Bot.Core.OpenDota.Models
{
    public class Rating
    {
        public long account_id { get; set; }
        public long match_id { get; set; }
        public int solo_competitive_rank { get; set; }
        public int competitive_rank { get; set; }
        public DateTime time { get; set; }
    }
}
