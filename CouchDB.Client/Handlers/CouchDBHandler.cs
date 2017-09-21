using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// CouchDB Handler, which is capable of sending raw requests to CouchDB.
    /// </summary>
    public sealed class CouchDBHandler : ICouchDBHandler, IDisposable
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBHandler"/> class.
        /// </summary>
        /// <param name="baseUrl">Base url to be used when sending requests.</param>
        public CouchDBHandler(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            Uri serverUri;
            if (!Uri.TryCreate(UrlHelper.CombineUrl(baseUrl, "/"), UriKind.Absolute, out serverUri))
                throw new FormatException("URL is not in valid format.");

            _http = new HttpClient();
            _http.BaseAddress = serverUri;
        }

        /// <summary>
        /// Disposes current instance of <see cref="CouchDBHandler"/>.
        /// </summary>
        public void Dispose()
        {
            _http.Dispose();
        }

        /// <summary>
        /// Sends request to CouchDB and returns response.
        /// </summary>
        /// <param name="relativeUrl">Relative url to the CouchDB endpoint.</param>
        /// <param name="requestMethod"><see cref="RequestMethod"/> to be used when sending request.</param>
        /// <param name="request">Instance of <see cref="Request"/> to be sent.</param>
        /// <returns>Instance of <see cref="Response"/> received from CouchDB.</returns>
        /// <exception cref="NotSupportedException">Request method is not supported.</exception>
        public async Task<Response> SendRequestAsync(string relativeUrl, RequestMethod requestMethod, Request request)
        {
            HttpResponseMessage httpResponseMessage;
            switch (requestMethod)
            {
                case RequestMethod.GET:
                    httpResponseMessage = await _http.GetAsync(relativeUrl).Safe();
                    break;
                case RequestMethod.PUT:
                    httpResponseMessage = await _http.PutAsync(relativeUrl, request?.ToHttpContent()).Safe();
                    break;
                case RequestMethod.DELETE:
                    httpResponseMessage = await _http.DeleteAsync(relativeUrl).Safe();
                    break;
                case RequestMethod.POST:
                    httpResponseMessage = await _http.PostAsync(relativeUrl, request?.ToHttpContent()).Safe();
                    break;
                default:
                    var errorMessage = $"Error while executing `{nameof(CouchDBHandler)}::{nameof(SendRequestAsync)}`: Request method {requestMethod} is not supported."
                        + $" Possibly uncovered member in `{nameof(RequestMethod)}` enum.";
                    throw new NotSupportedException(errorMessage);
            }

            return new Response(httpResponseMessage);
        }
    }
}
