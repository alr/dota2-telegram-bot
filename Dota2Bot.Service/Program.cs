using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core;
using Dota2Bot.Core.Bot;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.OpenDota;
using Dota2Bot.Core.SteamApi;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dota2Bot.Service
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static async Task Main(string[] args)
        {
            // global unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // config log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            await new HostBuilder()
                .ConfigureAppConfiguration(builder => builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Dota2BotService>();
                })
                .RunConsoleAsync();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logger.Fatal("Unhandled exception", ex);
        }
    }
}
