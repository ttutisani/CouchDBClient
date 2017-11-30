using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents response received from CouchDB.
    /// </summary>
    public sealed class Response
    {
        private readonly HttpResponseMessage _httpResponseMessage;

        /// <summary>
        /// Initializes new instance of <see cref="Response"/> class.
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        public Response(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null)
                throw new ArgumentNullException(nameof(httpResponseMessage));

            _httpResponseMessage = httpResponseMessage;
        }

        /// <summary>
        /// Reads response as JSON by deserializing it into given type.
        /// </summary>
        /// <typeparam name="TResult">Deserialization type.</typeparam>
        /// <param name="nullIfNotFound">Boolean indicating whether to convert Not Found (404) into null.</param>
        /// <returns>Deserialized object.</returns>
        public async Task<TResult> ReadAsAsync<TResult>(bool nullIfNotFound)
        {
            return await HttpClientHelper.HandleObjectResponse<TResult>(_httpResponseMessage, nullIfNotFound).Safe();
        }

        /// <summary>
        /// Reads response as string.
        /// </summary>
        /// <param name="nullIfNotFound">Boolean indicating whether to convert Not Found (404) into null.</param>
        /// <returns>String content read from the current instance of <see cref="Response"/>.</returns>
        public async Task<string> ReadAsStringAsync(bool nullIfNotFound)
        {
            return await HttpClientHelper.HandleStringResponse(_httpResponseMessage, nullIfNotFound).Safe();
        }

        /// <summary>
        /// Reads response as raw byte array.
        /// </summary>
        /// <param name="nullIfNotFound">Boolean indicating whether to convert Not Found (404) into null.</param>
        /// <returns>Byte array content read from the current instance of <see cref="Response"/>.</returns>
        public async Task<byte[]> ReadAsByteArrayAsync(bool nullIfNotFound)
        {
            return await HttpClientHelper.HandleRawResponse(_httpResponseMessage, nullIfNotFound).Safe();
        }

        /// <summary>
        /// Parses response to ensure there is no failure code received from server.
        /// </summary>
        /// <param name="nullIfNotFound">Boolean indicating whether to convert Not Found (404) into null.</param>
        /// <returns></returns>
        public async Task EnsureSuccessAsync(bool nullIfNotFound)
        {
            await HttpClientHelper.HandleVoidResponse(_httpResponseMessage, nullIfNotFound).Safe();
        }

        /// <summary>
        /// Retrieves underlying <see cref="HttpResponseMessage"/> object.
        /// </summary>
        /// <returns>Underlying <see cref="HttpResponseMessage"/>.</returns>
        public HttpResponseMessage GetHttpResponseMessage()
        {
            return _httpResponseMessage;
        }
    }
}
