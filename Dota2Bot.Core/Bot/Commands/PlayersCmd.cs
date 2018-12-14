using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;

namespace Dota2Bot.Core.Bot.Commands
{
    public class PlayersCmd : BaseCmd
    {
        public override string Cmd => "players";
        public override string Description => "list of players";

        public override async Task Execute(long chatId, string args)
        {
            using (DataManager dataManager = new DataManager(Config))
            {
                var players = dataManager.ChatGetPlayers(chatId);
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
}
