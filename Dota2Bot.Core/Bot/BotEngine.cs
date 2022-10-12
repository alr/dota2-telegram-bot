using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

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
            logger.LogError(exception, "Telegram Error");
            await Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
                { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            };

            await handler;
        }

        private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var messageText = message.Text;

            try
            {
                var command = CommandHelper.Parse(messageText);
                if (command == null)
                {
                    await PrintHello(chatId);
                    return;
                }
                
                var c = CreateCmd(command.Value.Cmd);
                if (c != null)
                    await c.Execute(chatId, command.Value.Args);
                else
                    await PrintHello(chatId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cmd: {Message}", messageText);
                await telegram.SendTextMessageAsync(chatId, "An error has occurred, please try again", 
                    cancellationToken: cancellationToken);
            }
        }

        private IBotCmd CreateCmd(string cmd)
        {
            var command = CommandHelper.GetCommands().FirstOrDefault(x => x.Cmd == cmd);

            if (command is BaseCmd baseCmd)
            {
                baseCmd.SetServiceProvider(serviceScopeFactory);
                baseCmd.SetTelegram(telegram);
            }

            return command;
        }

        private async Task PrintHello(long chatId)
        {
            await telegram.SendTextMessageAsync(chatId, "Hello there!");
        }
    }
}
