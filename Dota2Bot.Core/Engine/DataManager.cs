using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<Player> PlayerGet(long playerId)
        {
            return await dbContext.Players.FirstOrDefaultAsync(x => x.Id == playerId);
        }

        public async Task<TgChat> ChatGet(long chatId, params Expression<Func<TgChat, object>>[] includes)
        {
            return await dbContext.Chats.IncludeMultiple(includes).FirstOrDefaultAsync(x => x.Id == chatId);
        }

        public async Task<TgChat> ChatGetOrAdd(long chatId, params Expression<Func<TgChat, object>>[] includes)
        {
            var exists = await ChatGet(chatId, includes);
            if (exists == null)
            {
                var result = await dbContext.Chats.AddAsync(new TgChat {Id = chatId});
                return result.Entity;
            }
            else
            {
                return exists;
            }
        }

        public async Task<List<Player>> ChatGetPlayers(long chatId, params Expression<Func<Player, object>>[] includes)
        {
            return await dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .IncludeMultiple(includes)
                .ToListAsync();
        }
        
        public Task<bool> ChatAddPlayer(TgChat chat, Player player)
        {
            var exists = chat.ChatPlayers.Any(x => x.PlayerId == player.Id);
            if (exists == false)
            {
                chat.ChatPlayers.Add(new TgChatPlayers
                {
                    Player = player
                });
            }

            return Task.FromResult(!exists);
        }

        public Task<bool> ChatRemovePlayer(TgChat chat, Player player)
        {
            var chatPlayer = chat.ChatPlayers.FirstOrDefault(x => x.PlayerId == player.Id);
            if (chatPlayer != null)
            {
                return Task.FromResult(chat.ChatPlayers.Remove(chatPlayer));
            }

            return Task.FromResult(false);
        }

        public async Task<List<Match>> ChatGetMatches(long chatId, int limit, params Expression<Func<Match, object>>[] includes)
        {
            return await dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .SelectMany(x => x.Matches.OrderByDescending(k => k.MatchId).Take(limit))
                .IncludeMultiple(includes)
                .ToListAsync();
        }

        #region MatchNotifier

        public async Task<List<Match>> GetMathes(List<long> matchIds, params Expression<Func<Match, object>>[] includes)
        {
             return await dbContext.Matches
                    .IncludeMultiple(includes)
                    .Where(x => matchIds.Contains(x.MatchId))
                    .ToListAsync();
        }

        public async Task<List<TgChatPlayers>> GetChatPlayers(params Expression<Func<TgChatPlayers, object>>[] includes)
        {
            return await dbContext.ChatPlayers
                .IncludeMultiple(includes)
                .ToListAsync();
        }

        public async Task<List<Match>> GetPlayerMatches(long playerId, DateTime dateStartFrom, DateTime dateStartTo)
        {
            return await dbContext.Matches
                .Where(x => x.PlayerId == playerId
                        && x.DateStart >= dateStartFrom
                        && x.DateStart <= dateStartTo)
                .ToListAsync();
        }

        public async Task<List<Hero>> FindHerosByName(string name)
        {
            var lowerName = name.ToLower();
            
            return await dbContext.Heroes
                .Where(x => x.Name.ToLower().Contains(lowerName))
                .ToListAsync();
        }

        #endregion

        #region Reports

        public async Task<WeeklyReport> WeeklyReport(long chatId, DateTime dateStart, Hero hero)
        {
            var mathesQeury = dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
                .SelectMany(x => x.Matches.Where(k => k.DateStart >= dateStart));

            if (hero != null)
            {
                mathesQeury = mathesQeury.Where(x => x.HeroId == hero.Id);
            }

            var stats = (await mathesQeury
                .Include(x => x.Player)
                .ToListAsync())
                .GroupBy(x => x.PlayerId)
                .Select(g => new WeeklyPlayerModel
                {
                    PlayerId = g.Key,
                    Name = g.Max(x => x.Player.Name),
                    KillsAvg = (int) Math.Round(g.Average(x => x.Kills)),
                    KillsMax = g.Max(x => x.Kills),
                    Matches = g.Count(),
                    WinRate = g.Count() >= 1 ? g.Count(x => x.Won) * 100.0 / g.Count() : -1,
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

            var total = (await mathesQeury.ToListAsync())
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

        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }
    }
}
