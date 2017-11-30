using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    internal static class HttpClientHelper
    {
        internal async static Task<byte[]> HandleRawResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                if (convertNotFoundIntoNull && httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";
                throw new CouchDBClientException(errorMessage);
            }

            var contentAsBytes = await httpResponse.Content.ReadAsByteArrayAsync().Safe();
            return contentAsBytes;
        }

        /// <summary>
        /// Handle object response.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="convertNotFoundIntoNull"></param>
        /// <returns></returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        internal async static Task HandleVoidResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            await HandleObjectResponse<ServerResponseDTO>(httpResponse, convertNotFoundIntoNull).Safe();
        }

        /// <summary>
        /// Handle object response.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpResponse"></param>
        /// <param name="deserializer"></param>
        /// <param name="convertNotFoundIntoNull"></param>
        /// <returns></returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        internal async static Task<TResult> HandleObjectResponse<TResult>(HttpResponseMessage httpResponse, Func<string, TResult> deserializer, bool convertNotFoundIntoNull)
        {
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound && convertNotFoundIntoNull)
            {
                return default(TResult);
            }

            var responseJson = await httpResponse.Content.ReadAsStringAsync().Safe();
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    throw new CouchDBClientException(errorMessage);
                }

                var responseObject = JsonConvert.DeserializeObject<ServerResponseDTO>(responseJson);
                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject));
            }

            var resultObject = deserializer(responseJson);
            return resultObject;
        }

        /// <summary>
        /// Handle object response.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="httpResponse"></param>
        /// <param name="convertNotFoundIntoNull"></param>
        /// <returns></returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        internal async static Task<TResult> HandleObjectResponse<TResult>(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleObjectResponse(httpResponse, strJson => JsonConvert.DeserializeObject<TResult>(strJson), convertNotFoundIntoNull).Safe();
        }

        /// <summary>
        /// Handle object response.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="convertNotFoundIntoNull"></param>
        /// <returns></returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        internal async static Task<string> HandleStringResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleObjectResponse(httpResponse, strJson => strJson, convertNotFoundIntoNull).Safe();
        }
    }
}
