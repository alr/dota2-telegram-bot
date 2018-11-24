using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Domain;
using Dota2Bot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dota2Bot.Core.Engine
{
    public class DataManager : IDisposable
    {
        private Dota2DbContext dbContext;

        public DbSet<Hero> Heroes => dbContext.Heroes;
        public DbSet<Match> Matches => dbContext.Matches;
        public DbSet<Player> Players => dbContext.Players;

        public DataManager(IConfiguration config)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Dota2DbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DotaDb"));

            dbContext = new Dota2DbContext(optionsBuilder.Options);
        }

        public List<Player> GetPlayers(long chatId, params Expression<Func<Player, object>>[] includes)
        {
            return dbContext.ChatPlayers
                .Where(x => x.ChatId == chatId)
                .Select(x => x.Player)
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

        public int SaveChanges()
        {
            return dbContext.SaveChanges();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (dbContext != null)
                {
                    dbContext.Dispose();
                    dbContext = null;
                }
            }
        }

        #endregion
    }
}
