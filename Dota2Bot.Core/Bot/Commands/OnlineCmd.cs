using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.SteamApi;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class OnlineCmd : BaseCmd
    {
        public override string Cmd => "online";
        public override string Description => "in game users";

        public override async Task Execute(long chatId, string args)
        {
            SteamClient steamClient = new SteamClient(Config);

            using (DataManager dataManager = new DataManager(Config))
            {
                var players = dataManager.GetPlayers(chatId);
                var steamIds = players.Select(x => x.SteamId).ToList();
                
                var summaries = steamClient.GetPlayerSummaries(steamIds);


                var playersOnline = summaries.Where(x => x.gameid == SteamClient.Dota2GameId).ToList();
                var playersInGame = playersOnline.Where(x => x.gameserverip != null).ToList();

                if (playersOnline.Any() == false)
                {
                    await Telegram.SendTextMessageAsync(chatId, "No one is playing");
                }
                else
                {
                    var profilesInfo = players.Select(x => new
                    {
                        Name = x.Name,
                        IsOnline = playersOnline.Any(k => k.steamid == x.SteamId),
                        InGame = playersInGame.Any(k => k.steamid == x.SteamId)
                    }).ToList();

                    var online = String.Join("\r\n", profilesInfo.Where(x => x.IsOnline && !x.InGame).Select(x => x.Name.Markdown()).OrderBy(x => x));
                    var message = $"*Online:*\r\n{online}";

                    if (playersInGame.Any())
                    {
                        var ingame = String.Join("\r\n", profilesInfo.Where(x => x.IsOnline && x.InGame).Select(x => x.Name.Markdown()).OrderBy(x => x));
                        message += $"\r\n*In Game:*\r\n{ingame}";
                    }

                    await Telegram.SendTextMessageAsync(chatId, message, parseMode:ParseMode.Markdown);
                }
            }
        }
    }
}
