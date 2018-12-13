using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot;
using Dota2Bot.Core.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dota2Bot.Service
{
    public class Dota2BotService : IHostedService
    {
        private readonly IConfiguration config;

        private BotEngine bot;
        private Grabber grabber;

        public Dota2BotService(IConfiguration config)
        {
            this.config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            bot = new BotEngine(config);
            bot.Start();

            grabber = new Grabber(config);
            grabber.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            bot?.Stop();
            grabber?.Stop();

            return Task.CompletedTask;
        }
    }
}
