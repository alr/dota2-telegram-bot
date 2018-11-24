using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Microsoft.Extensions.Configuration;
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
        protected TelegramBotClient Telegram { get; set; }
        protected IConfiguration Config { get; set; }

        public abstract string Cmd { get; }
        public abstract string Description { get; }
        public abstract Task Execute(long chatId, string args);

        public void SetConfiguration(IConfiguration config)
        {
            this.Config = config;
        }

        public void SetTelegram(TelegramBotClient telegram)
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
