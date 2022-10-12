using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands;

public class HelpCmd : BaseCmd
{
    public override string Cmd => "help";
    public override string Description => "print help";
    
    protected override async Task ExecuteHandler(long chatId, string args)
    {
        var cmdList = CommandHelper.GetCommands()
            .Select(x => $"/{x.Cmd} - {x.Description}")
            .OrderBy(x => x);
        
        var cmdListStr = "*Available commands:*\r\n" + string.Join("\r\n", cmdList);
        
        await Telegram.SendTextMessageAsync(chatId, cmdListStr, parseMode: ParseMode.Markdown);
    }
}
