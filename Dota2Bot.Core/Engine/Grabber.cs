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

namespace Dota2Bot.Core.Engine
{
    public class Grabber
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Grabber));

        private readonly IConfiguration config;

        private readonly SteamClient steam;
        private readonly OpenDotaClient openDota;
        private readonly MatchNotifier matchNotifier;
        
        private List<int> heroesCacheIds;

        private Thread matchesThread;
        private Timer infoTimer;

        public Grabber(IConfiguration config)
        {
            this.config = config;

            steam = new SteamClient(config);
            openDota = new OpenDotaClient(config);
            matchNotifier = new MatchNotifier(config);
        }

        public void Start()
        {
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
                        logger.Error("MatchesThreadFunc", ex);
                    }

                    try
                    {
                        await OnlineThreadFunc();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("OnlineThreadFunc", ex);
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
            using (DataManager dataManager = new DataManager(config))
            {
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
        }

        private List<string> prevOnline;
        private Dictionary<string, PlayerGameSession> playersTimeCache = new Dictionary<string, PlayerGameSession>();

        private async Task OnlineThreadFunc()
        {
            using (DataManager dataManager = new DataManager(config))
            {
                var players = dataManager.Players.ToList();
                var steamIds = players.Select(x => x.SteamId).ToList();
                
                var currSummaries = steam.GetPlayerSummaries(steamIds);
                var currOnline = currSummaries.Where(x => x.gameid == SteamClient.Dota2GameId).Select(x => x.steamid).ToList();

                if (prevOnline == null)
                {
                    prevOnline = currOnline;
                    return;
                }

                var now = DateTime.UtcNow;
                
                var newOnline = currOnline.Except(prevOnline).ToList();
                var newOffline = prevOnline.Except(currOnline).ToList();

                prevOnline = currOnline;
                
                foreach (var online in newOnline)
                {
                    playersTimeCache[online] = new PlayerGameSession
                    {
                        SteamId = online,
                        Start = now
                    };
                }
                
                List<PlayerGameSession> endSessions = new List<PlayerGameSession>();
                foreach (var offline in newOffline)
                {
                    if (playersTimeCache.ContainsKey(offline))
                    {
                        var session = playersTimeCache[offline];
                        session.End = now;

                        endSessions.Add(session);

                        playersTimeCache.Remove(offline);
                    }
                    else
                    {
                        endSessions.Add(new PlayerGameSession
                        {
                            SteamId = offline
                        });
                    }
                }

                await matchNotifier.NotifyStatusChats(newOnline, endSessions);
            }
        }

        private async Task MatchesThreadFunc()
        {
            using (DataManager dataManager = new DataManager(config))
            {
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
            using (DataManager dataManager = new DataManager(config))
            {
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
            }

            CacheHeroes();
        }

        private void CacheHeroes()
        {
            using (DataManager dataManager = new DataManager(config))
            {
                heroesCacheIds = dataManager.Heroes.Select(x => x.Id).ToList();
            }
        }
    }
}
