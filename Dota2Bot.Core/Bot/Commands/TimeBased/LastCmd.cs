using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dota2Bot.Core.Bot.Commands.TimeBased;

public class LastCmd : BaseTimeReportCmd
{
    public override string Cmd => "last";
    public override string Description => "stats for N last matches";

    private int count;

    protected override async Task ExecuteHandler(long chatId, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            var placeholders = new int[] { 10, 100, 1000 }
                .Select(x => new [] { InlineKeyboardButton.WithCallbackData($"{x}", $"/{Cmd} {x}") });
                
            await Telegram.SendTextMessageAsync(chatId, "Please specify number of matches",
                replyMarkup: new InlineKeyboardMarkup(placeholders));
                
            return;
        }

        var components = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var countStr = components[0];

        if (!int.TryParse(countStr, out count) || count <= 0)
        {
            await Telegram.SendTextMessageAsync(chatId, "Wrong matches number");
            return;
        }

        var heroName = components.Length > 1 ? components[1] : null;

        var chat = await DataManager.ChatGet(chatId);
        if (chat == null)
            return;

        var heroResult = await GetHero(heroName);
        if (heroResult.IsFailed)
        {
            await base.HandleHeroError(chatId, heroResult.Errors.First());
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
        
    protected override string HandleHeroErrorCallback(string hero)
    {
        return $"/{Cmd} {count} {hero}";
    }
}