﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Engine;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class StreakCmd : BaseCmd
    {
        public override string Cmd => "streak";
        public override string Description => "show streaks";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            const int Max = 20; // max game streak
            const int Min = 3; // min game streak
            
            var mathesData = (await DataManager.ChatGetMatches(chatId, Max, x => x.Player))
                .GroupBy(x => x.PlayerId)
                .Select(x => new
                {
                    Player = x.First().Player,
                    Matches = x.OrderByDescending(k => k.MatchId).ToList()
                })
                .Where(x => x.Matches.Count > 1)
                .ToList();

            if (mathesData.Count == 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "No streaks");
                return;
            }

            var streakList = (
                from data in mathesData
                let lastMatch = data.Matches.First()
                let streakCount = data.Matches.TakeWhile(x => x.GameResult() == lastMatch.GameResult()).Count()
                select new
                {
                    Player = data.Player,
                    GameResult = lastMatch.GameResult(),
                    Count = streakCount,
                })
                .Where(x => x.Count >= Min)
                .OrderByDescending(x => x.Count)
                .ToList();

            var message = "*Streak:*\r\n" + String.Join("\r\n", streakList.Select(x =>
                String.Format("[{0}]({1}) | {2} | {3}",
                    x.Player.Name,
                    x.Player.Link(),
                    x.GameResult,
                    x.Count)));

            await Telegram.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
