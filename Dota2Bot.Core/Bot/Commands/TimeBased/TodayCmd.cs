using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Extensions;

namespace Dota2Bot.Core.Bot.Commands.TimeBased
{
    public class TodayCmd : BaseTimeReportCmd
    {
        public override string Cmd => "today";
        public override string Description => "today stats";

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).Date;
            return local.ConvertFromTimezone(timezone);
        }

        protected override string ParseHeroName(string args)
        {
            return args;
        }
    }
}
