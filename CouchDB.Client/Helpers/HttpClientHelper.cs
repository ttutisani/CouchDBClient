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
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound && convertNotFoundIntoNull)
            {
                return default(TResult);
            }

            var responseJsonTask = httpResponse.Content?.ReadAsStringAsync().Safe();
            var responseJson = responseJsonTask.HasValue
                ? await responseJsonTask.Value
                : null;
            if (!httpResponse.IsSuccessStatusCode)
            {
                ServerResponseDTO responseObject;

                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";
                if (string.IsNullOrWhiteSpace(responseJson) || !TryDeserializeObject(responseJson, out responseObject))
                {
                    throw new CouchDBClientException(errorMessage);
                }

                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject));
            }

            var resultObject = deserializer(responseJson);
            return resultObject;
        }

        private static bool TryDeserializeObject<TResult>(string json, out TResult result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<TResult>(json);
                return true;
            }
            catch { }

            result = default(TResult);
            return false;
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
