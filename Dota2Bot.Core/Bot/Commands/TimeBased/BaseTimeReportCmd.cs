using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine.Models.Report;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Domain.Entity;
using FluentResults;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public abstract class BaseTimeReportCmd : BaseCmd
{
    protected abstract DateTime GetDateStart(string timezone);
    protected abstract string ParseHeroName(string args);
    protected async Task<Result<Hero>> GetHero(string args)
    {
        var heroName = ParseHeroName(args);

        if (string.IsNullOrEmpty(heroName))
            return Result.Ok<Hero>(null);

        var heroes = await DataManager.FindHerosByName(heroName);

        if (heroes.Count == 0)
            return Result.Fail<Hero>("Hero not found");

        if (heroes.Count > 1)
            return Result.Fail<Hero>(new HeroParseError("Multiple heroes found",
                heroes.Select(x => x.Name)));

        return Result.Ok(heroes.Single());
    }
    protected virtual string HandleHeroErrorCallback(string hero)
    {
        return $"/{Cmd} {hero}";
    }
    protected async Task HandleHeroError(long chatId, IError error)
    {
        switch (error)
        {
            case HeroParseError heroParseError:
            {
                var replyKeyboardMarkup = heroParseError.Heroes
                    .Select(x => new[] { InlineKeyboardButton.WithCallbackData(x, HandleHeroErrorCallback(x)) });

                await Telegram.SendTextMessageAsync(chatId, heroParseError.Message,
                    replyMarkup: new InlineKeyboardMarkup(replyKeyboardMarkup));
                    
                break;
            }
            default:
            {
                await Telegram.SendTextMessageAsync(chatId, error.Message);
                break;
            }
        }
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
    protected override async Task ExecuteHandler(long chatId, string args)
    {
        var chat = await DataManager.ChatGet(chatId);
        if (chat == null)
            return;

        var dateStart = GetDateStart(chat.Timezone);

        var heroResult = await GetHero(args);
        if (heroResult.IsFailed)
        {
            await HandleHeroError(chatId, heroResult.Errors.First());
            return;
        }

        var report = await DataManager.WeeklyReport(chatId, dateStart, heroResult.Value);

        await PrintReport(chatId, report);
    }
}

class HeroParseError : Error
{
    public IEnumerable<string> Heroes { get; }
    public HeroParseError(string message, IEnumerable<string> heroes)
        : base(message)
    {
        Heroes = heroes;
    }
}
