using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dota2Bot.Core.Extensions;
using Dota2Bot.Core.OpenDota.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Dota2Bot.Core.OpenDota
{
    public class OpenDotaClient
    {
        public string BaseUrl = "https://api.opendota.com/api/";

        private const int RETRY_COUNT = 60;
        private const int RETRY_DELAY = 3000;

        private const int REQUESTS_DELAY = 1000;
        private DateTime lastRequestTime = DateTime.UtcNow.Date;

        private readonly object syncLock = new object();

        public OpenDotaClient(IConfiguration config)
        {
            
        }

        public List<Hero> Heroes()
        {
            RestRequest request = new RestRequest("heroes");

            return Execute<List<Hero>>(request) ?? new List<Hero>();
        }

        public Player Player(long id)
        {
            RestRequest request = new RestRequest("players/{id}");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<Player>(request);
        }

        public WinLose WinLose(long id)
        {
            RestRequest request = new RestRequest("players/{id}/wl");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<WinLose>(request);
        }

        public List<Match> Matches(long id, int? limit = null, int? offset = null)
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

            return Execute<List<Match>>(request) ?? new List<Match>();
        }

        public List<Rating> Ratings(long id)
        {
            RestRequest request = new RestRequest("players/{id}/ratings");
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<List<Rating>>(request) ?? new List<Rating>();
        }

        public int? Refresh(long id)
        {
            RestRequest request = new RestRequest("players/{id}/refresh", Method.POST);
            request.AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<Refresh>(request)?.length;
        }

        private T Execute<T>(RestRequest request) where T : class, new()
        {
            lock (syncLock)
            {
                // задержка между запросами
                if ((DateTime.UtcNow - lastRequestTime).TotalMilliseconds < REQUESTS_DELAY)
                    Thread.Sleep(REQUESTS_DELAY);

                RestClient client = new RestClient(BaseUrl);

                var response = client.Execute<T>(request, RETRY_COUNT, RETRY_DELAY);

                // задержка между запросами
                lastRequestTime = DateTime.UtcNow;

                return response.Data;
            }
        }
    }
}
