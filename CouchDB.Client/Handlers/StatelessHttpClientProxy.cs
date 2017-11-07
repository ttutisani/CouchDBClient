using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    internal sealed class StatelessHttpClientProxy : IStatelessHttpClient, IDisposable
    {
        private readonly HttpClient _http;

        public StatelessHttpClientProxy()
        {
            _http = new HttpClient();
        }

        public void Dispose()
        {
            _http.Dispose();
        }

        public async Task<Response> SendRequestAsync(string absoluteUrl, RequestMethod requestMethod, Request request)
        {
            HttpResponseMessage httpResponseMessage;
            switch (requestMethod)
            {
                case RequestMethod.GET:
                    httpResponseMessage = await _http.GetAsync(absoluteUrl).Safe();
                    break;
                case RequestMethod.PUT:
                    httpResponseMessage = await _http.PutAsync(absoluteUrl, request?.ToHttpContent()).Safe();
                    break;
                case RequestMethod.DELETE:
                    httpResponseMessage = await _http.DeleteAsync(absoluteUrl).Safe();
                    break;
                case RequestMethod.POST:
                    httpResponseMessage = await _http.PostAsync(absoluteUrl, request?.ToHttpContent()).Safe();
                    break;
                default:
                    var errorMessage = $"Error while executing `{nameof(HttpCouchDBHandler)}::{nameof(SendRequestAsync)}`: Request method {requestMethod} is not supported."
                        + $" Possibly uncovered member in `{nameof(RequestMethod)}` enum.";
                    throw new NotSupportedException(errorMessage);
            }

            return new Response(httpResponseMessage);
        }
    }
}
