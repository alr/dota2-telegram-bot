using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine.Models;
using Dota2Bot.Core.OpenDota;
using Dota2Bot.Core.SteamApi;
using Dota2Bot.Domain;
using Dota2Bot.Domain.Entity;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dota2Bot.Core.Engine
{
    public class Grabber
    {
        private readonly ILogger<Grabber> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly SteamClient steam;
        private readonly OpenDotaClient openDota;
        private readonly MatchNotifier matchNotifier;
        private readonly SteamAppsCache steamAppsCache;

        private List<int> heroesCacheIds;

        private Thread matchesThread;
        private Timer infoTimer;

        public Grabber(ILogger<Grabber> logger, IServiceScopeFactory serviceScopeFactory,
            SteamClient steam, OpenDotaClient openDota, MatchNotifier matchNotifier,
            SteamAppsCache steamAppsCache)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            
            this.steam = steam;
            this.openDota = openDota;
            
            this.matchNotifier = matchNotifier;
            this.steamAppsCache = steamAppsCache;
        }

        public void Start(CancellationToken cancellationToken)
        {
            UpdateHeroes();
            CacheHeroes();

            // thread for collect dota stats and send TG notifications
            matchesThread = new Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        await MatchesThreadFunc();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "MatchesThreadFunc");
                    }

                    try
                    {
                        await OnlineThreadFunc();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "OnlineThreadFunc");
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
            });

            matchesThread.IsBackground = true;
            matchesThread.Start();

            // thread for update user info
            infoTimer = new Timer(InfoThreadFunc);
            infoTimer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromHours(4));
        }

        public void Stop()
        {
            // no action
        }

        private void InfoThreadFunc(object state)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<DataManager>();
                
            var players = dataManager.Players.ToList();

            foreach (var player in players)
            {
                var info = openDota.Player(player.Id);
                if (info != null)
                {
                    player.Name = info.profile.personaname;
                    player.SteamId = info.profile.steamid;
                    player.SoloRank = info.solo_competitive_rank;
                    player.PartyRank = info.competitive_rank;
                    player.RankTier = info.rank_tier;

                    dataManager.SaveChanges();
                }                   
            }
        }

        private Dictionary<string, OnlineGrabber> onlineGrabbers = new Dictionary<string, OnlineGrabber>();

        private async Task OnlineThreadFunc()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<DataManager>();

            var players = dataManager.Players.ToList();
            var steamIds = players.Select(x => x.SteamId).ToList();

            var summary = steam.GetPlayerSummaries(steamIds);

            var currOnlineByGames = summary
                .Where(x => x.gameid != null)
                .GroupBy(x => x.gameid)
                .ToDictionary(k => k.Key, v => v.Select(x => x.steamid).ToList());

            var games = currOnlineByGames.Keys.ToList();
            var emptyGames = onlineGrabbers.Where(x => !games.Contains(x.Key)).ToList();

            foreach (var grabber in emptyGames)
            {
                await grabber.Value.CheckOnline(new List<string>());
            }

            bool updateGamesCache = false;

            foreach (var game in currOnlineByGames)
            {
                var gameId = game.Key;
                var currOnline = game.Value;

                if (onlineGrabbers.ContainsKey(gameId))
                {
                    await onlineGrabbers[gameId].CheckOnline(currOnline);
                }
                else
                {
                    var gameTitle = steamAppsCache.GetGameById(gameId);
                    if (gameTitle != null)
                    {
                        onlineGrabbers[gameId] = new OnlineGrabber(matchNotifier, gameTitle);
                        await onlineGrabbers[gameId].CheckOnline(currOnline);
                    }
                    else
                    {
                        updateGamesCache = true;
                    }
                }
            }

            if (updateGamesCache)
            {
                steamAppsCache.UpdateCache();
            }
        }

        private async Task MatchesThreadFunc()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<DataManager>();
            
            var players = dataManager.Players.ToList();

            // get new matches
            var matches = CollectMatches(players);
            if (matches.Count > 0)
            {
                //check heroes data
                var heroIds = matches.Select(x => x.HeroId).ToList();
                if (heroIds.Except(heroesCacheIds).Any())
                {
                    UpdateHeroes();
                }

                dataManager.Matches.AddRange(matches);
                dataManager.SaveChanges();

                //send to telegram
                var matchIds = matches.Select(x => x.MatchId).Distinct().ToList();
                await matchNotifier.NotifyChats(matchIds);
            }
        }

        public List<Match> CollectMatches(List<Player> players)
        {
            var playerIds = players.Select(x => x.Id).ToList();
            var matchIds = new HashSet<long>();

            foreach (var player in players)
            {
                var ids = GetMatchIds(player);
                matchIds.UnionWith(ids);
            }
            
            List<Match> matches = new List<Match>();
            foreach (var matchId in matchIds)
            {
                var match = steam.GetMatchDetails(matchId);
                var gamePlayers = match.players.Where(x => playerIds.Contains(x.account_id)).ToList();

                foreach (var player in gamePlayers)
                {
                    Match m = new Match
                    {
                        MatchId = match.match_id,
                        GameMode = match.game_mode,
                        LobbyType = match.lobby_type,
                        DateStart = match.DateStart(),
                        Duration = match.Duration(),
                        Won = match.Won(player),
                        PlayerId = player.account_id,
                        PlayerSlot = player.player_slot,
                        HeroId = player.hero_id,
                        Kills = player.kills,
                        Deaths = player.deaths,
                        Assists = player.assists,
                        LeaverStatus = player.leaver_status
                    };

                    matches.Add(m);
                }
            }

            foreach (var player in players)
            {
                var playerMatches = matches.Where(x => x.PlayerId == player.Id).ToList();
                UpdatePlayerByMatches(player, playerMatches);
            }

            return matches;
        }

        private void UpdatePlayerByMatches(Player player, List<Match> matches)
        {
            if (matches.Any(x => x.PlayerId != player.Id))
                throw new InvalidOperationException("matches.Any(x => x.PlayerId != player.Id)");

            var latest = matches.OrderByDescending(x => x.MatchId).FirstOrDefault();
            if (latest != null)
            {
                player.LastMatchId = latest.MatchId;
                player.LastMatchDate = latest.DateStart + latest.Duration;

                player.WinCount += matches.Count(x => x.Won);
                player.LoseCount += matches.Count(x => !x.Won);
            }
        }

        private List<long> GetMatchIds(Player player)
        {
            var result = steam.GetMatchHistory(player.Id);
            if (result != null)
            {
                var m = result.matches.TakeWhile(x => x.match_id != player.LastMatchId) //todo where?
                    .ToList();

                return m.Select(x => x.match_id).ToList();
            }

            return new List<long>();
        }

        void UpdateHeroes()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<DataManager>();
            
            var heroesLocal = dataManager.Heroes.ToDictionary(k => k.Id);
            var heroesRemote = steam.GetHeroes().heroes.Select(x => new Hero
            {
                Id = x.id,
                Name = x.localized_name
            }).ToDictionary(k => k.Id);

            // добавляем Unknown Hero для поддержки связи с матчами, которые не были сыграны
            heroesRemote[0] = new Hero {Id = 0, Name = "Unknown Hero"};
            
            foreach (var heroRemoteKey in heroesRemote.Keys)
            {
                var hero = heroesRemote[heroRemoteKey];

                if (heroesLocal.ContainsKey(heroRemoteKey))
                {
                    heroesLocal[heroRemoteKey].Name = hero.Name;
                }
                else
                {
                    dataManager.Heroes.Add(hero);
                }
            }

            dataManager.SaveChanges();

            CacheHeroes();
        }

        private void CacheHeroes()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dataManager = scope.ServiceProvider.GetRequiredService<DataManager>();
            
            heroesCacheIds = dataManager.Heroes.Select(x => x.Id).ToList();
        }
    }
}
