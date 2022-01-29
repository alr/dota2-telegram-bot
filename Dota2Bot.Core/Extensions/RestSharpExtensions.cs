using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace Dota2Bot.Core.Extensions
{
    public static class RestSharpExtensions
    {
        public static RestRequest AddQueryParameter(this RestRequest request, string name, double value)
        {
            return request.AddQueryParameter(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public static bool EnsureSuccessStatusCode(this RestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.Accepted || 
                response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            return false;
        }

        public static async Task<RestResponse<T>> Execute<T>(this RestClient client, RestRequest request, int rertyCount, int retryDelay)
            where T : class, new()
        {
            RestResponse<T> response = null;

            for (int i = 0; i < rertyCount; i++)
            {
                // задержку между попытками делаем тут,
                // потому что на последней неудачной попытке нам не нужно делать ждать
                if (i > 0)
                {
                    Thread.Sleep(retryDelay);
                }

                response = await client.ExecuteAsync<T>(request);

                // проверяем необходим ли повтор запроса
                if (!ShouldRetryRequest(client, request, response))
                {
                    break;
                }
            }

            // общая обработка ответа
            HandleResponse(client, request, response);

            return response;
        }

        public static async Task<RestResponse> Execute(this RestClient client, RestRequest request, int rertyCount, int retryDelay)
        {
            RestResponse response = null;

            for (int i = 0; i < rertyCount; i++)
            {
                // задержку между попытками делаем тут,
                // потому что на последней неудачной попытке нам не нужно делать ждать
                if (i > 0)
                {
                    Thread.Sleep(retryDelay);
                }

                response = await client.ExecuteAsync(request);

                // проверяем необходим ли повтор запроса
                if (!ShouldRetryRequest(client, request, response))
                {
                    break;
                }
            }

            // общая обработка ответа
            HandleResponse(client, request, response);

            return response;
        }

        private static bool ShouldRetryRequest(RestClient client, RestRequest request, RestResponse response)
        {
            // сетевая ошибка
            if (response.ErrorException != null)
                return true;

            // серверная недоступность
            if ((int)response.StatusCode >= 500)
                return true;

            // слишком частые запросы
            if ((int)response.StatusCode == 429)
                return true;

            return false;
        }

        private static void HandleResponse(RestClient client, RestRequest request, RestResponse response)
        {
            if (!EnsureSuccessStatusCode(response))
            {
                var path = client.BuildUri(request).AbsoluteUri;

                //ILog log = LogManager.GetLogger(typeof(RestSharpExtensions));
                //log.ErrorFormat("BadStatus: {0} {1}", path, (int)response.StatusCode);
            }
        }
    }
}
