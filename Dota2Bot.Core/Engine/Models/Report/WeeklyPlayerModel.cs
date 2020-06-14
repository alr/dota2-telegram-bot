using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2Bot.Core.Engine.Models.Report
{
    public class WeeklyPlayerModel
    {
        public long PlayerId { get; set; }

        public string Name { get; set; }
        public int KillsAvg { get; set; }
        public int KillsMax { get; set; }
        public int Matches { get; set; }
        public double WinRate { get; set; }

        public double? WinRateDiff { get; set; }

        public double TotalTime { get; set; }

        public int TotalKill { get; set; }
        public int TotalDeath { get; set; }
        public int TotalAssist { get; set; }
    }
}
