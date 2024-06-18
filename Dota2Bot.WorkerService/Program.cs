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
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Dota2Bot.WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use block body for methods", Justification = "<Pending>")]
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((c, l) =>
                {
                    l.AddSentry();
                })
                .ConfigureServices((hostContext, services) =>
                { 
                    services.AddDbContext<Dota2DbContext>(
                        builder => builder.UseNpgsql(hostContext.Configuration.GetConnectionString("DB")), 
                        ServiceLifetime.Transient);

                    services.AddTransient<DataManager>();
                    services.AddTransient<SteamClient>(provider => new SteamClient(hostContext.Configuration["STEAM_API_KEY"]));
                    services.AddTransient<OpenDotaClient>();
                    services.AddTransient<MatchNotifier>();
                    services.AddTransient<ITelegramBotClient>(provider => new TelegramBotClient(hostContext.Configuration["TELEGRAM_API_KEY"]));
                    
                    services.AddSingleton<BotEngine>();
                    services.AddSingleton<Grabber>();
                    services.AddSingleton<SteamAppsCache>();
                    
                    services.AddHostedService<Worker>();
                });
    }
}
