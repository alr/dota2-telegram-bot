using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.Extensions;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class WeekCmd : BaseCmd
    {
        public override string Cmd => "week";
        public override string Description => "weekly stats";

        public override async Task Execute(long chatId, string args)
        {
            using (DataManager dataManager = new DataManager(Config))
            {
                var chat = dataManager.ChatGet(chatId);
                if (chat == null)
                    return;
                
                var dateStart = GetDateStart(chat.Timezone);

                var report = dataManager.WeeklyReport(chatId, dateStart);
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

        private DateTime GetDateStart(string timezone)
        {
            var weekStartLocal = DateTime.UtcNow.ConvertToTimezone(timezone).StartOfWeek(DayOfWeek.Monday);
            return weekStartLocal.ConvertFromTimezone(timezone);
        }
    }
}
