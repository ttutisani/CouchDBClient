using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    internal static class HttpClientHelper
    {
        internal async static Task HandleResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            await HandleResponse<CouchDBServer.ServerResponseDTO>(httpResponse, convertNotFoundIntoNull);
        }

        internal async static Task<TResult> HandleResponse<TResult>(HttpResponseMessage httpResponse, Func<string, TResult> deserializer, bool convertNotFoundIntoNull)
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
                if (convertNotFoundIntoNull && responseObject.Error == ServerResponseError.Not_Found)
                {
                    return default(TResult);
                }
                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject), null);
            }

            var resultObject = deserializer(responseJson);
            return resultObject;
        }

        internal async static Task<TResult> HandleResponse<TResult>(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleResponse(httpResponse, strJson => JsonConvert.DeserializeObject<TResult>(strJson), convertNotFoundIntoNull);
        }

        internal async static Task<string> HandleStringResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleResponse(httpResponse, strJson => strJson, convertNotFoundIntoNull);
        }
    }
}
