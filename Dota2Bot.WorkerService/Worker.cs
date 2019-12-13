using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot;
using Dota2Bot.Core.Engine;
using Dota2Bot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dota2Bot.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly Dota2DbContext dota2DbContext;
        private readonly BotEngine botEngine;
        private readonly Grabber grabber;

        public Worker(Dota2DbContext dota2DbContext, BotEngine botEngine, Grabber grabber)
        {
            this.dota2DbContext = dota2DbContext;
            this.botEngine = botEngine;
            this.grabber = grabber;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //await dota2DbContext.Database.EnsureCreatedAsync(stoppingToken);
                await dota2DbContext.Database.MigrateAsync(cancellationToken: stoppingToken);

                botEngine.Start(stoppingToken);
                grabber.Start(stoppingToken);

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