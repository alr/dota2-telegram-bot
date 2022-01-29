using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dota2Bot.Core.SteamApi;

namespace Dota2Bot.Core.Engine
{
    public class SteamAppsCache
    {
        private readonly SteamClient steam;
        
        private Dictionary<string, string> steamApps = new Dictionary<string, string>();

        public SteamAppsCache(SteamClient steam)
        {
            this.steam = steam;
        }

        public async Task Init()
        {
            var games = await steam.GetGames();
            if (games != null) 
            {
                steamApps = games
                    .GroupBy(x => x.appid)
                    .ToDictionary(k => k.Key, v => v.First().name);
            }
        }

        public async Task UpdateCache()
        {
            await Init();
        }

        public string GetGameById(string gameId)
        {
            return steamApps.ContainsKey(gameId) ? steamApps[gameId] : null;
        }
    }
}
