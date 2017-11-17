using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    internal class HttpCouchDBHandler : ICouchDBHandler, IDisposable
    {
        private readonly string _baseUrl;
        private readonly IStatelessHttpClient _http;

        public HttpCouchDBHandler(string baseUrl, IStatelessHttpClient http)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            if (http == null)
                throw new ArgumentNullException(nameof(http));

            _baseUrl = UrlHelper.CombineUrl(baseUrl, "/");
            _http = http;
        }

        public void Dispose()
        {
            (_http as IDisposable)?.Dispose();
        }

        public async Task<Response> SendRequestAsync(string relativeUrl, RequestMethod requestMethod, Request request)
        {
            var absoluteUrl = UrlHelper.CombineUrl(_baseUrl, relativeUrl);

            return await _http.SendRequestAsync(absoluteUrl, requestMethod, request).Safe();
        }
    }
}
