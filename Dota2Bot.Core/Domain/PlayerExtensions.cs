using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Domain.Entity;

namespace Dota2Bot.Core.Domain
{
    public static class PlayerExtensions
    {
        public static string Link(this Player player)
        {
            return "http://www.dotabuff.com/players/" + player.Id;
        }
    }
}
