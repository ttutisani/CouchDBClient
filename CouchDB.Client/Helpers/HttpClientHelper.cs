using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    internal static class HttpClientHelper
    {
        internal async static Task HandleResponse(HttpResponseMessage httpResponse)
        {
            await HandleResponse<CouchDBServer.ServerResponseDTO>(httpResponse);
        }

        internal async static Task<TResult> HandleResponse<TResult>(HttpResponseMessage httpResponse, Func<string, TResult> deserializer)
        {
            var responseJson = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    throw new CouchDBClientException(errorMessage, null, null);
                }

                var responseObject = JsonConvert.DeserializeObject<CouchDBServer.ServerResponseDTO>(responseJson);
                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject), null);
            }

            var resultObject = deserializer(responseJson);
            return resultObject;
        }

        internal async static Task<TResult> HandleResponse<TResult>(HttpResponseMessage httpResponse)
        {
            return await HandleResponse(httpResponse, strJson => JsonConvert.DeserializeObject<TResult>(strJson));
        }

        internal async static Task<string> HandleStringResponse(HttpResponseMessage httpResponse)
        {
            return await HandleResponse(httpResponse, strJson => strJson);
        }
    }
}
