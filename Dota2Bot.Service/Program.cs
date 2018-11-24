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

namespace Dota2Bot.Service
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            // global unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // config log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            // setup service
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            BotEngine bot = new BotEngine(config);
            bot.Start();

            Grabber grabber = new Grabber(config);
            grabber.Start();

            Console.ReadLine();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logger.Fatal("Unhandled exception", ex);
        }
    }
}
