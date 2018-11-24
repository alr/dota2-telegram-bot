using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.SteamApi.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Dota2Bot.Core.SteamApi
{
public class SteamClient
    {
        public string BaseUrl = "http://api.steampowered.com/";

        private const int RETRY_COUNT = 3;
        private const int RETRY_DELAY = 3000;

        private const int REQUESTS_DELAY = 1000;
        private DateTime lastRequestTime = DateTime.UtcNow.Date;

        private readonly object syncLock = new object();

        private readonly string apiKey;

        public const string Dota2GameId = "570";

        public SteamClient(IConfiguration config)
        {
            this.apiKey = config["steam.ApiKey"];
        }

        public List<PlayerSummariesResponse.Player> GetPlayerSummaries(List<string> steamIds)
        {
            RestRequest request = new RestRequest("ISteamUser/GetPlayerSummaries/v0002/");
            request.AddQueryParameter("steamids", String.Join(",", steamIds));

            var response = Execute<PlayerSummariesResponse>(request);
            if (response?.response != null)
                return response.response.players;

            return new List<PlayerSummariesResponse.Player>();
        }

        public MatchHistoryResponse.Result GetMatchHistory(long accountId, int count = 100)
        {
            RestRequest request = new RestRequest("IDOTA2Match_570/GetMatchHistory/V001/");
            request.AddQueryParameter("account_id", accountId);
            request.AddQueryParameter("matches_requested", count);
            
            var response = Execute<MatchHistoryResponse>(request);
            
            return response?.result;
        }

        public MatchDetailsResponse.Result GetMatchDetails(long matchId)
        {
            RestRequest request = new RestRequest("IDOTA2Match_570/GetMatchDetails/V001/");
            request.AddQueryParameter("match_id", matchId);
            
            var response = Execute<MatchDetailsResponse>(request);
            
            return response?.result;
        }

        public HeroesResponse.Result GetHeroes(string language = "en_us")
        {
            RestRequest request = new RestRequest("IEconDOTA2_570/GetHeroes/v1");
            request.AddQueryParameter("language", language);

            var response = Execute<HeroesResponse>(request);

            return response?.result;
        }

        private T Execute<T>(RestRequest request) where T : class, new()
        {
            lock (syncLock)
            {
                // задержка между запросами
                if ((DateTime.UtcNow - lastRequestTime).TotalMilliseconds < REQUESTS_DELAY)
                    Thread.Sleep(REQUESTS_DELAY);

                // add api key
                request.AddQueryParameter("key", apiKey);

                RestClient client = new RestClient(BaseUrl);

                var response = client.Execute<T>(request, RETRY_COUNT, RETRY_DELAY);

                // задержка между запросами
                lastRequestTime = DateTime.UtcNow;

                if (response != null && response.Data != null)
                    return response.Data;

                return null;
            }
        }
    }
}
