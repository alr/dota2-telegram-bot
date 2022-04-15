using System;
using System.Collections.Generic;
using System.Text;
using Dota2Bot.Domain.Entity;

namespace Dota2Bot.Core.Engine.Models.Report
{
    public class WeeklyReport
    {
        public List<WeeklyPlayerModel> Players { get; set; }
        public WeeklyOverall Overall { get; set; }

        public Hero Hero { get; set; }
    }
}
