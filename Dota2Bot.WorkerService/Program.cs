using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dota2Bot.Core.Bot;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.OpenDota;
using Dota2Bot.Core.SteamApi;
using Dota2Bot.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Dota2Bot.WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                { 
                    services.AddDbContext<Dota2DbContext>(
                        builder => builder.UseNpgsql(hostContext.Configuration.GetConnectionString("DotaDb")), 
                        ServiceLifetime.Transient);

                    services.AddTransient<DataManager>();
                    services.AddTransient<SteamClient>(provider => new SteamClient(hostContext.Configuration["steam.ApiKey"]));
                    services.AddTransient<OpenDotaClient>();
                    services.AddTransient<MatchNotifier>();
                    services.AddTransient<ITelegramBotClient>(provider => new TelegramBotClient(hostContext.Configuration["telegram.ApiKey"]));
                    
                    services.AddSingleton<BotEngine>();
                    services.AddSingleton<Grabber>();
                    
                    services.AddHostedService<Worker>();
                });
    }
}