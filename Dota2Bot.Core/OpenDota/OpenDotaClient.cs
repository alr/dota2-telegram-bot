using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.OpenDota.Models;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Dota2Bot.Core.OpenDota
{
    public class OpenDotaClient
    {
        private const string BaseUrl = "https://api.opendota.com/api/";

        private const int RETRY_COUNT = 60;
        private const int RETRY_DELAY = 3000;

        private const int REQUESTS_DELAY = 1000;
        private DateTime lastRequestTime = DateTime.UtcNow.Date;

        private readonly AsyncLock syncLock = new AsyncLock();
        
        private readonly RestClient client;

        public OpenDotaClient()
        {
            client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
        }

        public async Task<List<Hero>> Heroes()
        {
            RestRequest request = new RestRequest("heroes");

            return await Execute<List<Hero>>(request) ?? new List<Hero>();
        }

        public async Task<Player> Player(long id)
        {
            RestRequest request = new RestRequest("players/{id}");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return await Execute<Player>(request);
        }

        public async Task<WinLose> WinLose(long id)
        {
            RestRequest request = new RestRequest("players/{id}/wl");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return await Execute<WinLose>(request);
        }

        public async Task<List<Match>> Matches(long id, int? limit = null, int? offset = null)
        {
            RestRequest request = new RestRequest("players/{id}/matches");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var props = typeof(Match).GetProperties().Select(x => x.Name);
            foreach (var prop in props)
            {
                request.AddParameter("project", prop, ParameterType.QueryString);
            }

            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.QueryString);

            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.QueryString);

            return (await Execute<List<Match>>(request)) ?? new List<Match>();
        }

        public async Task<MatchDetailsResponse.Result> GetMatchDetails(long matchId)
        {
            RestRequest request = new RestRequest("/matches/{match_id}");
            request.AddParameter("match_id", matchId, ParameterType.UrlSegment);

            var response = await Execute<MatchDetailsResponse.Result>(request);
            return response;
        }

        public async Task<List<Rating>> Ratings(long id)
        {
            RestRequest request = new RestRequest("players/{id}/ratings");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return (await Execute<List<Rating>>(request)) ?? new List<Rating>();
        }

        public async Task<int?> Refresh(long id)
        {
            RestRequest request = new RestRequest("players/{id}/refresh", Method.Post);
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return (await Execute<Refresh>(request))?.length;
        }

        private async Task<T> Execute<T>(RestRequest request) where T : class, new()
        {
            using (await syncLock.LockAsync())
            {
                // задержка между запросами
                if ((DateTime.UtcNow - lastRequestTime).TotalMilliseconds < REQUESTS_DELAY)
                    Thread.Sleep(REQUESTS_DELAY);                

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
