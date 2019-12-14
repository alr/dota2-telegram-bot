using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dota2Bot.Core.Engine.Models.Report;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Domain;
using Dota2Bot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dota2Bot.Core.Engine
{
    public class DataManager
    {
        private readonly Dota2DbContext dbContext;

        public DbSet<Hero> Heroes => dbContext.Heroes;
        public DbSet<Match> Matches => dbContext.Matches;
        public DbSet<Player> Players => dbContext.Players;
        public DbSet<Rating> Ratings => dbContext.Ratings;

        public DataManager(Dota2DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Player PlayerGet(long playerId)
        {
            return dbContext.Players.FirstOrDefault(x => x.Id == playerId);
        }

        public TgChat ChatGet(long chatId, params Expression<Func<TgChat, object>>[] includes)
        {
            return dbContext.Chats.IncludeMultiple(includes).FirstOrDefault(x => x.Id == chatId);
        }

        public TgChat ChatGetOrAdd(long chatId, params Expression<Func<TgChat, object>>[] includes)
        {
            var exists = ChatGet(chatId, includes);
            if (exists == null)
            {
                return dbContext.Chats.Add(new TgChat {Id = chatId}).Entity;
            }
            else
            {
                return exists;
            }
        }

        public List<Player> ChatGetPlayers(long chatId, params Expression<Func<Player, object>>[] includes)
        {
            return dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .IncludeMultiple(includes)
                .ToList();
        }
        
        public bool ChatAddPlayer(TgChat chat, Player player)
        {
            var exists = chat.ChatPlayers.Any(x => x.PlayerId == player.Id);
            if (exists == false)
            {
                chat.ChatPlayers.Add(new TgChatPlayers
                {
                    Player = player
                });
            }

            return !exists;
        }

        public bool ChatRemovePlayer(TgChat chat, Player player)
        {
            var chatPlayer = chat.ChatPlayers.FirstOrDefault(x => x.PlayerId == player.Id);
            if (chatPlayer != null)
            {
                return chat.ChatPlayers.Remove(chatPlayer);
            }

            return false;
        }

        public List<Match> ChatGetMatches(long chatId, int limit, params Expression<Func<Match, object>>[] includes)
        {
            return dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .SelectMany(x => x.Matches.OrderByDescending(k => k.MatchId).Take(limit))
                .IncludeMultiple(includes)
                .ToList();
        }

        #region MatchNotifier

        public List<Match> GetMathes(List<long> matchIds, params Expression<Func<Match, object>>[] includes)
        {
             return dbContext.Matches
                    .IncludeMultiple(includes)
                    .Where(x => matchIds.Contains(x.MatchId))
                    .ToList();
        }

        public List<TgChatPlayers> GetChatPlayers(params Expression<Func<TgChatPlayers, object>>[] includes)
        {
            return dbContext.ChatPlayers
                .IncludeMultiple(includes)
                .ToList();
        }

        public List<Match> GetPlayerMatches(long playerId, DateTime dateStartFrom, DateTime dateStartTo)
        {
            return dbContext.Matches
                .Where(x => x.PlayerId == playerId
                        && x.DateStart >= dateStartFrom
                        && x.DateStart <= dateStartTo)
                .ToList();
        }

        #endregion

        #region Reports

        public WeeklyReport WeeklyReport(long chatId, DateTime dateStart)
        {
            var mathesQeury = dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .SelectMany(x => x.Matches.Where(k => k.DateStart >= dateStart));

            var stats = mathesQeury
                .Include(x => x.Player)
                .ToList()
                .GroupBy(x => x.PlayerId)
                .Select(g => new WeeklyPlayerModel
                {
                    PlayerId = g.Key,
                    Name = g.Max(x => x.Player.Name),
                    KillsAvg = (int) Math.Round(g.Average(x => x.Kills)),
                    KillsMax = g.Max(x => x.Kills),
                    Matches = g.Count(),
                    WinRate = g.Count() >= 3 ? g.Count(x => x.Won) * 100.0 / g.Count() : -1,
                    TotalTime = g.Sum(x => x.Duration.TotalSeconds),
                    TotalKill = g.Sum(x => x.Kills),
                    TotalDeath = g.Sum(x => x.Deaths),
                    TotalAssist = g.Sum(x => x.Assists)
                })
                .OrderByDescending(x => x.WinRate)
                .ThenByDescending(x => x.Matches)
                .ThenByDescending(x => x.KillsAvg)
                .ThenByDescending(x => x.KillsMax)
                .ToList();

            if (stats.Count == 0)
            {
                return null;
            }

            var total = mathesQeury.ToList()
                .GroupBy(x => x.MatchId)
                .Select(g => new
                {
                    MatchId = g.Key,
                    Won = g.Any(x => x.Won)
                }).ToList();

            WeeklyReport viewModel = new WeeklyReport
            {
                Players = stats,
                Overall = new WeeklyOverall
                {
                    Total = total.Count,
                    Wins = total.Count(x => x.Won),
                    WinRate = total.Count >= 3 ? total.Count(x => x.Won)*100.0/total.Count : -1
                }
            };

            return viewModel;
        }

        #endregion

        public int SaveChanges()
        {
            return dbContext.SaveChanges();
        }
    }
}
