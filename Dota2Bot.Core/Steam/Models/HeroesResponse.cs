using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Dota2Bot.Core.SteamApi.Models
{
    public class HeroesResponse
    {
        public class Hero
        {
            public string name { get; set; }
            public int id { get; set; }
            public string localized_name { get; set; }
        }

        public class Result
        {
            public List<Hero> heroes { get; set; }
            public int status { get; set; }
            public int count { get; set; }

            public Result()
            {
                heroes = new List<Hero>();
            }
        }

        public Result result { get; set; }
    }
}
