using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public abstract class BaseTimeReportCmd : BaseCmd
    {
        protected abstract DateTime GetDateStart(string timezone);

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var chat = DataManager.ChatGet(chatId);
            if (chat == null)
                return;
            
            var dateStart = GetDateStart(chat.Timezone);

            var report = DataManager.WeeklyReport(chatId, dateStart);
            if (report == null)
            {
                await Telegram.SendTextMessageAsync(chatId, "No one played :(");
                return;
            }

            string msg = "*#* | " +
                         "*Nick* | " +
                         "*WinRate* | " +
                         //"*KD* |" +
                         "*Time*\r\n";

            for (int i = 0; i < report.Players.Count; i++)
            {
                var player = report.Players[i];

                TimeSpan t = TimeSpan.FromSeconds(player.TotalTime);
                string time = String.Format("{0:N1}", t.TotalHours);

                var str = $"{i + 1} | " +
                          $"{player.Name.Markdown()} | " +
                          $"{(player.WinRate == -1 ? "N/A" : player.WinRate.ToString("N2"))} | " +
                          //$"{player.TotalKill} / {player.TotalDeath} | " +
                          $"{time} ({player.Matches})\r\n";

                msg += str;
            }

            await Telegram.SendTextMessageAsync(chatId, msg, ParseMode.Markdown);
        }
    }

    public class TodayCmd : BaseTimeReportCmd
    {
        public override string Cmd => "today";
        public override string Description => "today stats";
        
        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).Date;
            return local.ConvertFromTimezone(timezone);
        }
    }

    public class WeekCmd : BaseTimeReportCmd
    {
        public override string Cmd => "week";
        public override string Description => "weekly stats";

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfWeek(DayOfWeek.Monday);
            return local.ConvertFromTimezone(timezone);
        }
    }

    public class MonthCmd : BaseTimeReportCmd
    {
        public override string Cmd => "month";
        public override string Description => "monthly stats";

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfMonth();
            return local.ConvertFromTimezone(timezone);
        }
    }

    public class YearCmd : BaseTimeReportCmd
    {
        public override string Cmd => "year";
        public override string Description => "year stats";

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfYear();
            return local.ConvertFromTimezone(timezone);
        }
    }

    public class DaysCmd : BaseTimeReportCmd
    {
        public override string Cmd => "days";
        public override string Description => "stats for N last days";

        private int days;

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            if (!int.TryParse(args, out days) || days <= 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "Wrong days number");
                return;
            }

            await base.ExecuteHandler(chatId, args);
        }

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).Date.AddDays(-days);
            return local.ConvertFromTimezone(timezone);
        }
    }
}
