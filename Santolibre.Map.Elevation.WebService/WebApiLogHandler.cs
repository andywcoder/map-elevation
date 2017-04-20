using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Santolibre.Map.Elevation.WebService
{
    public class WebApiLogHandler : DelegatingHandler
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri.PathAndQuery;

            if (request.Content != null)
            {
                await request.Content.ReadAsStringAsync().ContinueWith(task =>
                {
                    string logMessage;
                    string headerJson = null;

                    headerJson = SerializeHeaders(request.Headers);
                    logMessage = request.Method + " " + url + ", Header=" + headerJson + ", Body=" + task.Result;

                    Logger.Trace(logMessage);
                }, cancellationToken);
            }

            return await base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {
                var response = task.Result;
                var logLevel = ((int)response.StatusCode).ToString().StartsWith("5") ? LogLevel.Error : LogLevel.Info;
                var logMessage = "Response StatusCode=" + response.StatusCode + ", ReasonPhrase=" + response.ReasonPhrase;

                Logger.Log(logLevel, logMessage, task.Exception);

                return response;
            }, cancellationToken);
        }

        private string SerializeHeaders(HttpHeaders headers)
        {
            var excludeList = new List<string>() { "Cache-Control", "Connection", "Accept", "Accept-Encoding", "Accept-Language", "Host" };
            var headerDictionary = new Dictionary<string, string>();

            foreach (var item in headers.Where(x => !excludeList.Contains(x.Key)).ToList())
            {
                if (item.Value != null)
                {
                    var headerValue = string.Empty;
                    foreach (var value in item.Value)
                    {
                        headerValue += value + " ";
                    }
                    headerValue = headerValue.TrimEnd(' ');

                    headerDictionary.Add(item.Key, headerValue);
                }
            }

            return string.Join(", ", headerDictionary.Select(x => x.Key + "=" + x.Value));
        }
    }
}
