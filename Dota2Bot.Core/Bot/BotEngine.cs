using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;

namespace Dota2Bot.Core.Bot
{
    public class BotEngine
    {
        private readonly ILogger<BotEngine> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        
        private readonly ITelegramBotClient telegram;

        private CancellationTokenSource cts;

        public BotEngine(ILogger<BotEngine> logger, IServiceScopeFactory serviceScopeFactory, 
            ITelegramBotClient telegram)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.telegram = telegram;
        }

        public async Task Start()
        {
            cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new [] { UpdateType.Message, UpdateType.EditedMessage }
            };

            telegram.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken);

            await Task.CompletedTask;
        }

        public void Stop()
        {
            cts.Cancel();
        }

        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                //await botClient.SendTextMessageAsync(123, apiRequestException.ToString());
                await Task.CompletedTask;
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            if (message == null || message.Chat == null || message.Type != MessageType.Text)
                return;

            if (string.IsNullOrEmpty(message.Text) || message.Text.Length < 2)
                return;

            var chatId = message.Chat.Id;

            try
            {
                var text = message.Text.Substring(1);

                if (!string.IsNullOrEmpty(text))
                {
                    var data = text.Split(new[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);

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
                logger.LogError(ex,$"Cmd: {message.Text}");
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
                baseCmd.SetServiceProvider(serviceScopeFactory);
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
