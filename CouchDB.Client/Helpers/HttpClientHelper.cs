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

        internal async static Task HandleVoidResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            await HandleObjectResponse<ServerResponseDTO>(httpResponse, convertNotFoundIntoNull).Safe();
        }

        internal async static Task<TResult> HandleObjectResponse<TResult>(HttpResponseMessage httpResponse, Func<string, TResult> deserializer, bool convertNotFoundIntoNull)
        {
            var responseJson = await httpResponse.Content.ReadAsStringAsync().Safe();
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    throw new CouchDBClientException(errorMessage);
                }

                var responseObject = JsonConvert.DeserializeObject<ServerResponseDTO>(responseJson);
                if (convertNotFoundIntoNull && CommonError.Not_Found.EqualsErrorString(responseObject.Error))
                {
                    return default(TResult);
                }
                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject));
            }

            var resultObject = deserializer(responseJson);
            return resultObject;
        }

        internal async static Task<TResult> HandleObjectResponse<TResult>(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleObjectResponse(httpResponse, strJson => JsonConvert.DeserializeObject<TResult>(strJson), convertNotFoundIntoNull).Safe();
        }

        internal async static Task<string> HandleStringResponse(HttpResponseMessage httpResponse, bool convertNotFoundIntoNull)
        {
            return await HandleObjectResponse(httpResponse, strJson => strJson, convertNotFoundIntoNull).Safe();
        }
    }
}
