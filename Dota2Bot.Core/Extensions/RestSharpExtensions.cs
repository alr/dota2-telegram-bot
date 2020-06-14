using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using RestSharp;

namespace Dota2Bot.Core.Extensions
{
    public static class RestSharpExtensions
    {
        public static IRestRequest AddQueryParameter(this IRestRequest request, string name, double value)
        {
            return request.AddQueryParameter(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public static bool EnsureSuccessStatusCode(this IRestResponse response)
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

        public static IRestResponse<T> Execute<T>(this IRestClient client, IRestRequest request, int rertyCount, int retryDelay)
            where T : class, new()
        {
            IRestResponse<T> response = null;

            for (int i = 0; i < rertyCount; i++)
            {
                // задержку между попытками делаем тут,
                // потому что на последней неудачной попытке нам не нужно делать ждать
                if (i > 0)
                {
                    Thread.Sleep(retryDelay);
                }

                response = client.Execute<T>(request);

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

        public static IRestResponse Execute(this IRestClient client, IRestRequest request, int rertyCount, int retryDelay)
        {
            IRestResponse response = null;

            for (int i = 0; i < rertyCount; i++)
            {
                // задержку между попытками делаем тут,
                // потому что на последней неудачной попытке нам не нужно делать ждать
                if (i > 0)
                {
                    Thread.Sleep(retryDelay);
                }

                response = client.Execute(request);

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

        private static bool ShouldRetryRequest(IRestClient client, IRestRequest request, IRestResponse response)
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

        private static void HandleResponse(IRestClient client, IRestRequest request, IRestResponse response)
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
