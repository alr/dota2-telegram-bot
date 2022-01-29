using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot;
using Dota2Bot.Core.Engine;
using Dota2Bot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dota2Bot.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private BotEngine botEngine;
        private Grabber grabber;

        public Worker(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                botEngine = scope.ServiceProvider.GetRequiredService<BotEngine>();
                grabber = scope.ServiceProvider.GetRequiredService<Grabber>();
                
                var steamAppsCache = scope.ServiceProvider.GetRequiredService<SteamAppsCache>();    
                var dota2DbContext = scope.ServiceProvider.GetRequiredService<Dota2DbContext>();
                
                await dota2DbContext.Database.MigrateAsync(cancellationToken: stoppingToken);

                await steamAppsCache.Init();
                    
                await botEngine.Start();
                await grabber.Start();
                    
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
                await Task.CompletedTask;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            botEngine?.Stop();
            grabber?.Stop();

            return Task.CompletedTask;
        }
    }
}
