using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2Bot.Core.Engine.Models.Report
{
    public class WeeklyReport
    {
        public List<WeeklyPlayerModel> Players { get; set; }
        public WeeklyOverall Overall { get; set; }
    }
}
