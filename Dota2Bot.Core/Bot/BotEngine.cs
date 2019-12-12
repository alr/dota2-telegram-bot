using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot.Commands;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot
{
    public class BotEngine
    {
        private readonly ILogger<BotEngine> logger;
        private readonly IServiceProvider serviceProvider;
        
        private readonly ITelegramBotClient telegram;

        public BotEngine(ILogger<BotEngine> logger, IServiceProvider serviceProvider, 
            ITelegramBotClient telegram)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.telegram = telegram;

            telegram.OnMessage += BotOnMessageReceived;
            telegram.OnMessageEdited += BotOnMessageReceived;
        }

        public void Start(CancellationToken cancellationToken)
        {
            telegram.StartReceiving(cancellationToken: cancellationToken);
        }

        public void Stop()
        {
            telegram.StopReceiving();
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var update = messageEventArgs.Message;

            if (update == null || update.Chat == null || update.Type != MessageType.Text)
                return;

            if (string.IsNullOrEmpty(update.Text) || update.Text.Length < 2)
                return;

            var chatId = update.Chat.Id;

            try
            {
                var message = update.Text.Substring(1);

                if (!string.IsNullOrEmpty(message))
                {
                    var data = message.Split(new[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);

                    var cmd = data[0].Split('@')[0].ToLower().Trim(); // убираем имя бота и получаем команду
                    var args = data.Length == 2 ? data[1].Trim() : null;

                    var command = CreateCmd(cmd);
                    if (command != null)
                    {
                        await command.Execute(chatId, args);
                    }
                    else if (cmd == "help")
                    {
                        await PrintHelp(chatId);
                    }
                    else
                    {
                        await PrintHello(chatId);
                    }
                }
                else
                {
                    await PrintHello(chatId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Cmd: {update.Text}");
                await telegram.SendTextMessageAsync(chatId, "An error has occurred, please try again");
            }
        }

        private List<IBotCmd> GetCommands()
        {
            List<IBotCmd> commands = new List<IBotCmd>();

            var type = typeof(IBotCmd);
            var types = type.Assembly.GetTypes()
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();

            foreach (var commandType in types)
            {
                var constructorInfo = commandType.GetConstructors().First();

                var parameters = constructorInfo.GetParameters();
                var ctorParmas = parameters.Select(x => (object)null).ToArray();

                commands.Add(constructorInfo.Invoke(ctorParmas) as IBotCmd);
            }

            return commands;
        }

        private IBotCmd CreateCmd(string cmd)
        {
            var command = GetCommands().FirstOrDefault(x => x.Cmd == cmd);

            if (command is BaseCmd baseCmd)
            {
                baseCmd.SetServiceProvider(serviceProvider);
                baseCmd.SetTelegram(telegram);
            }

            return command;
        }

        private async Task PrintHelp(long chatId)
        {
            var cmdList = GetCommands().Select(x => String.Format("/{0} - {1}", x.Cmd, x.Description));
            var cmdListStr = "*Available commands:*\r\n" + String.Join("\r\n", cmdList);
            await telegram.SendTextMessageAsync(chatId, cmdListStr, parseMode: ParseMode.Markdown);
        }

        private async Task PrintHello(long chatId)
        {
            await telegram.SendTextMessageAsync(chatId, "Hello there!");
        }
    }
}
