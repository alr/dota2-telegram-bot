using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Telegram.Bot;

namespace Dota2Bot.Core.Bot.Commands
{
    public class PlayersCmd : BaseCmd
    {
        public override string Cmd => "players";
        public override string Description => "list of players";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var players = DataManager.ChatGetPlayers(chatId);
            if (players.Count > 0)
            {
                var msg = String.Join("\r\n", players.OrderBy(x => x.Id).Select(x => x.Id + " - " + x.Name));
                await Telegram.SendTextMessageAsync(chatId, msg);
            }
            else
            {
                await Telegram.SendTextMessageAsync(chatId, "No players");
            }
        }
    }
}
