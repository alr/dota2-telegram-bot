using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.OpenDota;
using Dota2Bot.Core.SteamApi;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Dota2Bot.Core.Bot.Commands
{
    public interface IBotCmd
    {
        string Cmd { get; }
        string Description { get; }

        Task Execute(long chatId, string args);
    }

    public abstract class BaseCmd : IBotCmd
    {
        private IServiceScopeFactory serviceScopeFactory;
        
        protected ITelegramBotClient Telegram { get; private set; }
        protected DataManager DataManager { get; private set; }
        protected OpenDotaClient OpenDota { get; private set; }
        protected SteamClient SteamClient { get; private set; }
        protected SteamAppsCache SteamAppsCache { get; private set; }
        
        protected abstract Task ExecuteHandler(long chatId, string args);

        public abstract string Cmd { get; }
        public abstract string Description { get; }

        public async Task Execute(long chatId, string args)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                this.DataManager = scope.ServiceProvider.GetRequiredService<DataManager>();
                this.OpenDota = scope.ServiceProvider.GetRequiredService<OpenDotaClient>();
                this.SteamClient = scope.ServiceProvider.GetRequiredService<SteamClient>();
                this.SteamAppsCache = scope.ServiceProvider.GetRequiredService<SteamAppsCache>();
                
                await ExecuteHandler(chatId, args);
            }
        }

        public void SetServiceProvider(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }
        
        public void SetTelegram(ITelegramBotClient telegram)
        {
            this.Telegram = telegram;
        }

        protected long GetPlayerId(string msg)
        {
            //try get id from link
            var regex = new Regex(@"dotabuff.com/players/(?<id>\d+)", RegexOptions.IgnoreCase);
            var match = regex.Match(msg);

            var idStr = match.Success ? match.Groups["id"].Value : msg;

            //test by number
            long id;
            if (long.TryParse(idStr, out id))
            {
                id = SteamId.ConvertTo32(id);
                return id;
            }

            return 0;
        }
    }
}
