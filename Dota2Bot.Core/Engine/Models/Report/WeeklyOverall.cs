using System;
using System.Collections.Generic;
using System.Text;

namespace Dota2Bot.Core.Engine.Models.Report
{
    public class WeeklyOverall
    {
        public double WinRate { get; set; }
        public int Wins { get; set; }
        public int Total { get; set; }

        public DateTime Date { get; set; }
    }
}
