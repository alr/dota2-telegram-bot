using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Engine;
using Humanizer;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    class StatCmd: BaseCmd
    {
        public override string Cmd => "stat";
        public override string Description => "show statistics for all players";

        public override async Task Execute(long chatId, string args)
        {
            using (DataManager dataManager = new DataManager(Config))
            {
                var players = dataManager.ChatGetPlayers(chatId)
                    .OrderByDescending(x => x.WinRate()).ToList();

                List<string> summary = new List<string>();
                for (int i = 0; i < players.Count; i++)
                {
                    var x = players[i];

                    double? winsToBeatPrev = null;
                    if (i > 0)
                    {
                        var prev = players[i - 1];
                        double? prevWinRate = prev.WinCount * 1.0 / (prev.TotalGames());

                        winsToBeatPrev = (prevWinRate * x.TotalGames() - x.WinCount) / (1.0 - prevWinRate);
                    }

                    var line = String.Format("[{0}]({1}) | {2:N2}% | {3} | +{4} | {5}",
                        x.Name,
                        x.Link(),
                        x.WinRate().ToString(CultureInfo.InvariantCulture),
                        RankTier.Get(x.RankTier).Replace("[", @"\["),
                        winsToBeatPrev.HasValue ? (int) Math.Ceiling(winsToBeatPrev.Value) : 0,
                        x.LastMatchDate.HasValue ? x.LastMatchDate.Value.Humanize() : null);

                    summary.Add(line);
                }

                var result = String.Join("\r\n", summary);

                if (String.IsNullOrEmpty(result))
                {
                    await Telegram.SendTextMessageAsync(chatId, "No players");
                    return;
                }

                await Telegram.SendTextMessageAsync(chatId, result, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            }
        }
    }
}
