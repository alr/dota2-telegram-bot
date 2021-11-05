using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.SteamApi;
using Dota2Bot.Core.SteamApi.Models;
using Dota2Bot.Domain.Entity;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class OnlineCmd : BaseCmd
    {
        public override string Cmd => "online";
        public override string Description => "in game users";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            var players = DataManager.ChatGetPlayers(chatId);
            var steamIds = players.Select(x => x.SteamId).ToList();
            
            var summaries = SteamClient.GetPlayerSummaries(steamIds);

            var playersOnlineByGames = summaries
                .Where(x => x.gameid != null)
                .GroupBy(x => x.gameid)
                .ToDictionary(k => k.Key, v => v.ToList());

            if (!playersOnlineByGames.Any())
            {
                await Telegram.SendTextMessageAsync(chatId, "No one is playing");
                return;
            }

            StringBuilder message = new StringBuilder();

            foreach(var game in playersOnlineByGames)
            {
                var gameId = game.Key;
                var playersOnline = game.Value;

                var gameTitle = SteamAppsCache.GetGameById(gameId) ?? "Unknown game";

                var msg = GetMessageForGame(gameTitle, players, playersOnline);
                
                message.Append(msg);
                message.AppendLine();
            }            

            await Telegram.SendTextMessageAsync(chatId, message.ToString(), parseMode:ParseMode.Markdown);
        }

        private string GetMessageForGame(string game, List<Player> players, List<PlayerSummariesResponse.Player> playersOnline)
        {
            var playersInGame = playersOnline.Where(x => x.gameserverip != null).ToList();

            var profilesInfo = players.Select(x => new
            {
                Name = x.Name,
                IsOnline = playersOnline.Any(k => k.steamid == x.SteamId),
                InGame = playersInGame.Any(k => k.steamid == x.SteamId)
            }).ToList();

            var online = String.Join("\r\n", profilesInfo.Where(x => x.IsOnline && !x.InGame).Select(x => x.Name.Markdown()).OrderBy(x => x));
            var message = $"*{game.Markdown()}:*\r\n{online}";

            if (playersInGame.Any())
            {
                var ingame = String.Join("\r\n", profilesInfo.Where(x => x.IsOnline && x.InGame).Select(x => x.Name.Markdown()).OrderBy(x => x));
                message += $"\r\n*{game.Markdown()} (In Game):*\r\n{ingame}";
            }

            return message;
        }
    }
}
