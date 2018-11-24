using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Engine.Models;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Domain.Entity;
using Humanizer;
using Humanizer.Localisation;
using log4net;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Engine
{
    public class MatchNotifier
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MatchNotifier));

        private readonly IConfiguration config;
        private readonly TelegramBotClient telegram;

        public MatchNotifier(IConfiguration config)
        {
            this.config = config;

            var telegramKey = config["telegram.ApiKey"];
            telegram = new TelegramBotClient(telegramKey);
        }

        #region Match
        
        public async Task NotifyChats(List<long> matchIds)
        {
            using (DataManager dataManager = new DataManager(config))
            {
                var matches = dataManager
                    .GetMathes(matchIds,
                        x => x.Player,
                        x => x.Player.ChatPlayers,
                        x => x.Hero)
                    .OrderBy(x => x.DateStart + x.Duration)
                    .ThenBy(x => x.PlayerSlot);

                //split matches to chats
                Dictionary<long, List<Match>> chats = new Dictionary<long, List<Match>>();
                foreach (var match in matches)
                {
                    foreach (var chatPlayer in match.Player.ChatPlayers)
                    {
                        if (chats.ContainsKey(chatPlayer.ChatId))
                            chats[chatPlayer.ChatId].Add(match);
                        else
                            chats[chatPlayer.ChatId] = new List<Match> { match };
                    }
                }

                //send messages to chats
                foreach (var chat in chats)
                {
                    await ProcessChat(chat.Key, chat.Value);
                }
            }
        }

        private async Task ProcessChat(long chatId, List<Match> matches)
        {
            // analyse matches
            var games = matches.GroupBy(x => x.MatchId).Select(g => new
            {
                MatchId = g.Key,
                Players = g.ToList()
            }).ToList();

            // send
            foreach (var game in games)
            {
                try
                {
                    var msg = FormatGame(game.Players);
                    await telegram.SendTextMessageAsync(chatId, msg);
                }
                catch (Exception ex)
                {
                    logger.Error("MatchId: " + game.MatchId, ex);
                }
            }
        }

        private string FormatGame(List<Match> players)
        {
            if (players.Count == 1)
            {
                var match = players.First();
                return FormatGameOne(match);
            }

            if (players.Count > 1)
            {
                return FormatGameMany(players);
            }

            return null;
        }

        private string FormatGameOne(Match match)
        {
            return String.Format("{0} [{1}] {2} with KDA {3}, {4} {5} \r\n {6}",
                match.Player.Name, match.Hero.Name, match.WonStr(), match.KdaStr(), match.LobbyTypeStr(),
                match.GameModeStr(), match.Link());
        }

        private string FormatGameMany(List<Match> players)
        {
            var wonSide = players.Where(x => x.Won).ToList();
            var loseSide = players.Where(x => !x.Won).ToList();

            var won = FormatManySide(wonSide);
            var lose = FormatManySide(loseSide);

            return String.Format("{0}\r\n\r\n{1}", won, lose).Trim();
        }

        private string FormatManySide(List<Match> players)
        {
            var game = players.FirstOrDefault();
            if (game == null)
                return null;

            var teammates = FormatManyPlayers(players);

            return String.Format("{0}\r\n\r\n{1}, {2} {3}\r\n\r\n{4}",
                teammates, game.WonStr(), game.LobbyTypeStr(), game.GameModeStr(), game.Link());
        }

        private string FormatManyPlayers(List<Match> players)
        {
            var teammates = players
                .Select(m => String.Format("{0} [{1}] with KDA {2}", m.Player.Name, m.Hero.Name, m.KdaStr()))
                .ToList();

            return String.Join("\r\n", teammates);
        }

        #endregion

        #region Online

        public async Task NotifyStatusChats(List<string> newOnline, List<PlayerGameSession> newOffline)
        {
            if (newOnline.Count == 0 && newOffline.Count == 0)
                return;
            
            using (DataManager dataManager = new DataManager(config))
            {
                var chats = dataManager
                    .GetChatPlayers(x => x.Chat, x => x.Player)
                    .GroupBy(x => x.Chat)
                    .Select(g => new
                    {
                        Chat = g.Key,
                        Players = g.Select(x => x.Player).ToList()
                    })
                    .ToList();

                foreach (var session in newOffline)
                {
                    if (session.Start != null && session.End != null)
                    {
                        var steamId = long.Parse(session.SteamId);
                        var playerId = SteamId.ConvertTo32(steamId);

                        var games = dataManager
                            .GetPlayerMatches(playerId, session.Start.Value, session.End.Value)
                            .Select(x => new { x.Won })
                            .ToList();

                        if (games.Count > 0)
                        {
                            var wons = games.Count(x => x.Won);
                            var losts = games.Count(x => !x.Won);

                            session.Stat = wons - losts;
                        }
                    }
                }
                
                foreach (var chat in chats)
                {
                    var online = chat.Players.Where(x => newOnline.Contains(x.SteamId)).ToList();
                    if (online.Count > 0)
                    {
                        var lines = online.Select(x => $"*{x.Name.Markdown()}* connected");
                        var msg = string.Join("\n", lines);

                        await telegram.SendTextMessageAsync(chat.Chat.Id, msg, parseMode: ParseMode.Markdown);
                    }
                    
                    List<PlayerGameSession> offline = new List<PlayerGameSession>();
                    foreach (var player in chat.Players)
                    {
                        var session = newOffline.FirstOrDefault(x => x.SteamId == player.SteamId);
                        if (session != null)
                        {
                            session.PlayerName = player.Name;        
                            offline.Add(session);
                        }
                    }

                    if (offline.Count > 0)
                    {
                        var lines = offline.Select(x =>
                        {
                            var str = $"*{x.PlayerName.Markdown()}* disconnected";

                            if (x.Stat != null)
                                str += $" || {x.Stat.Value:+#;-#;0}";

                            var duration = x.End - x.Start;
                            if (duration != null)
                            {
                                var time = duration.Value.Humanize(precision: 2, 
                                    minUnit: TimeUnit.Minute,
                                    maxUnit: TimeUnit.Hour);

                                str += $" || {time}";
                            }

                            return str;
                        });
                        
                        var msg = string.Join("\n", lines);

                        await telegram.SendTextMessageAsync(chat.Chat.Id, msg, parseMode: ParseMode.Markdown);
                    }
                }
            }
        }

        #endregion
    }
}
