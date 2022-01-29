using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class RemoveCmd : BaseCmd
    {
        public override string Cmd => "remove";
        public override string Description => "remove subscription to dotabuff profile by id";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                await Telegram.SendTextMessageAsync(chatId, "Please specify dotabuff link or id");
                return;
            }

            var playerId = GetPlayerId(args);
            if (playerId == 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "Invalid dotabuff link or id");
                return;
            }
            
            var player = DataManager.PlayerGet(playerId);
            if (player == null)
            {
                await Telegram.SendTextMessageAsync(chatId, "Player not found");
                return;
            }

            var chat = DataManager.ChatGet(chatId, x => x.ChatPlayers);
            if (chat != null)
            {
                DataManager.ChatRemovePlayer(chat, player);
                DataManager.SaveChanges();
            }

            await Telegram.SendTextMessageAsync(chatId, $"Player *{player.Name}* successfully removed", parseMode: ParseMode.Markdown);
        }
    }
}
