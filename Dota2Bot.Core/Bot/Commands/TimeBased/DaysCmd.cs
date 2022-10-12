using System;
using System.Linq;
using System.Threading.Tasks;
using Dota2Bot.Core.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public class DaysCmd : BaseTimeReportCmd
{
    public override string Cmd => "days";
    public override string Description => "stats for N last days";

    private int days;
    private string heroName;

    protected override async Task ExecuteHandler(long chatId, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            var placeholders = new int[] { 7, 14, 30, 90, 180 }
                .Select(x => new [] { InlineKeyboardButton.WithCallbackData($"{x}", $"/{Cmd} {x}") });

            await Telegram.SendTextMessageAsync(chatId, "Please specify number of days", 
                replyMarkup: new InlineKeyboardMarkup(placeholders));
                
            return;
        }

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

    protected override string HandleHeroErrorCallback(string hero)
    {
        return $"/{Cmd} {days} {hero}";
    }
}