using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.Engine.Models.Report;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Domain.Entity;
using FluentResults;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public abstract class BaseTimeReportCmd : BaseCmd
    {
        protected abstract DateTime GetDateStart(string timezone);

        protected abstract string ParseHeroName(string args);

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var chat = await DataManager.ChatGet(chatId);
            if (chat == null)
                return;

            var dateStart = GetDateStart(chat.Timezone);

            var heroResult = await GetHero(args);
            if (heroResult.IsFailed)
            {
                await Telegram.SendTextMessageAsync(chatId, heroResult.Errors.First().Message);
                return;
            }

            var report = await DataManager.WeeklyReport(chatId, dateStart, heroResult.Value);

            await PrintReport(chatId, report);
        }

        protected async Task PrintReport(long chatId, WeeklyReport report)
        {
            if (report == null)
            {
                await Telegram.SendTextMessageAsync(chatId, "No one played :(");
                return;
            }

            string msg = "";

            if (report.Hero != null)
                msg += $"*Hero:* {report.Hero.Name.Markdown()}\r\n";

            msg += "*#* | " +
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

        protected async Task<Result<Hero>> GetHero(string args)
        {
            var heroName = ParseHeroName(args);

            if (string.IsNullOrEmpty(heroName))
                return Result.Ok<Hero>(null);

            var heros = await DataManager.FindHerosByName(heroName);

            if (heros == null)
                return Result.Fail<Hero>("Hero not found");

            if (heros.Count > 1)
                return Result.Fail<Hero>("Multiple heroes found:" +
                    "\r\n - " + string.Join("\r\n - ", heros.Select(x => x.Name)));

            return Result.Ok(heros.Single());
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

        protected override string ParseHeroName(string args)
        {
            return args;
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

        protected override string ParseHeroName(string args)
        {
            return args;
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

        protected override string ParseHeroName(string args)
        {
            return args;
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

        protected override string ParseHeroName(string args)
        {
            return args;
        }
    }

    public class DaysCmd : BaseTimeReportCmd
    {
        public override string Cmd => "days";
        public override string Description => "stats for N last days";

        private int days;
        private string heroName;

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var components = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var daysStr = components[0];

            if (!int.TryParse(daysStr, out days) || days <= 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "Wrong days number");
                return;
            }

            heroName = components.Length > 1 ? components[1] : null;

            await base.ExecuteHandler(chatId, args);
        }

        protected override DateTime GetDateStart(string timezone)
        {
            var local = DateTime.UtcNow.ConvertToTimezone(timezone).Date.AddDays(-days);
            return local.ConvertFromTimezone(timezone);
        }

        protected override string ParseHeroName(string args)
        {
            return heroName;
        }
    }

    public class LastCmd : BaseTimeReportCmd
    {
        public override string Cmd => "last";
        public override string Description => "stats for N last matches";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var components = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var countStr = components[0];

            if (!int.TryParse(countStr, out int count) || count <= 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "Wrong days number");
                return;
            }

            var heroName = components.Length > 1 ? components[1] : null;

            var chat = await DataManager.ChatGet(chatId);
            if (chat == null)
                return;

            var heroResult = await GetHero(heroName);
            if (heroResult.IsFailed)
            {
                await Telegram.SendTextMessageAsync(chatId, heroResult.Errors.First().Message);
                return;
            }

            var report = await DataManager.LastReport(chatId, count, heroResult.Value);

            await PrintReport(chatId, report);
        }

        protected override DateTime GetDateStart(string timezone)
        {
            throw new NotImplementedException();
        }

        protected override string ParseHeroName(string args)
        {
            return args;
        }
    }

}
