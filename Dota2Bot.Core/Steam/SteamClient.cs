using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.SteamApi.Models;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Dota2Bot.Core.SteamApi
{
public class SteamClient
    {
        private const string BaseUrl = "http://api.steampowered.com/";

        private const int RETRY_COUNT = 3;
        private const int RETRY_DELAY = 3000;

        private const int REQUESTS_DELAY = 1000;
        private DateTime lastRequestTime = DateTime.UtcNow.Date;

        private readonly AsyncLock syncLock = new AsyncLock();

        private readonly string apiKey;
        
        public const string Dota2GameId = "570";

        private readonly RestClient client;

        public SteamClient(string apiKey)
        {
            this.apiKey = apiKey;

            client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
        }

        public async Task<List<PlayerSummariesResponse.Player>> GetPlayerSummaries(List<string> steamIds)
        {
            RestRequest request = new RestRequest("ISteamUser/GetPlayerSummaries/v0002/");
            request.AddQueryParameter("steamids", String.Join(",", steamIds));

            var response = await ExecuteAsync<PlayerSummariesResponse>(request);
            if (response?.response != null)
                return response.response.players;

            return null;
        }

        public async Task<MatchHistoryResponse.Result> GetMatchHistory(long accountId, int count = 100)
        {
            RestRequest request = new RestRequest("IDOTA2Match_570/GetMatchHistory/V001/");
            request.AddQueryParameter("account_id", accountId);
            request.AddQueryParameter("matches_requested", count);
            
            var response = await ExecuteAsync<MatchHistoryResponse>(request);
            
            return response?.result;
        }

        public async Task<MatchDetailsResponse.Result> GetMatchDetails(long matchId)
        {
            RestRequest request = new RestRequest("IDOTA2Match_570/GetMatchDetails/V001/");
            request.AddQueryParameter("match_id", matchId);
            
            var response = await ExecuteAsync<MatchDetailsResponse>(request);
            
            return response?.result;
        }

        public async Task<HeroesResponse.Result> GetHeroes(string language = "en_us")
        {
            RestRequest request = new RestRequest("IEconDOTA2_570/GetHeroes/v1");
            request.AddQueryParameter("language", language);

            var response = await ExecuteAsync<HeroesResponse>(request);

            return response?.result;
        }

        public async Task<List<SteamApp>> GetGames()
        {
            RestRequest request = new RestRequest("ISteamApps/GetAppList/v0002/");
            
            var response = await ExecuteAsync<GamesResponse>(request);

            return response?.applist.apps;
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request) where T : class, new()
        {
            using(await syncLock.LockAsync())
            {
                // задержка между запросами
                if ((DateTime.UtcNow - lastRequestTime).TotalMilliseconds < REQUESTS_DELAY)
                    Thread.Sleep(REQUESTS_DELAY);

                // add api key
                request.AddQueryParameter("key", apiKey);                

                var response = await client.Execute<T>(request, RETRY_COUNT, RETRY_DELAY);

                // задержка между запросами
                lastRequestTime = DateTime.UtcNow;

                if (response != null && response.Data != null)
                    return response.Data;

                return null;
            }
        }
    }
}
