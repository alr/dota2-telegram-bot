using System;
using System.Collections.Generic;
using System.Text;
using Dota2Bot.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace Dota2Bot.Domain
{
    public class Dota2DbContext : DbContext
    {
        public static readonly ILoggerFactory DebugLoggerFactory =
            LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter(level => level >= LogLevel.Information)
                    .AddDebug();
            });

        public Dota2DbContext(DbContextOptions<Dota2DbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(DebugLoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Match>()
                .HasKey(x => new { x.MatchId, x.PlayerId });

            modelBuilder.Entity<TgChatPlayers>()
                .HasKey(x => new { x.ChatId, x.PlayerId });
        }

        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        public DbSet<TgChat> Chats { get; set; }
        public DbSet<TgChatPlayers> ChatPlayers { get; set; }
    }
}
