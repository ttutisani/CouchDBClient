using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace CouchDB.Client.Tests
{
    public static class RequestIs
    {
        public static Request Empty()
        {
            return It.Is<Request>(req => req == Request.Empty);
        }

        internal static Request JsonString(string documentJson)
        {
            Predicate<Request> isString = req => 
            {
                var httpContent = req.ToHttpContent();

                return httpContent.ReadAsStringAsync().Result == documentJson
                    && httpContent.Headers.ContentType.CharSet == Encoding.UTF8.HeaderName
                    && httpContent.Headers.ContentType.MediaType == "application/json";
            };

            return It.Is<Request>(req => isString(req));
        }

        internal static Request JsonObject(object json)
        {
            Predicate<Request> isJson = req => 
            {
                var stringJson = req.ToHttpContent().ReadAsStringAsync().Result;
                return AssertHelper.StringIsJsonObject(stringJson, json);
            };

            return It.Is<Request>(req => isJson(req));
        }

        internal static Request ByteArray(byte[] expectedByteArray)
        {
            Predicate<Request> isByteArray = req => 
            {
                var actualByteArray = req.ToHttpContent().ReadAsByteArrayAsync().Result;
                if ((expectedByteArray == null) != (actualByteArray == null))
                    return false;

                if (expectedByteArray?.Length != actualByteArray?.Length)
                    return false;

                for (int i = 0; i < expectedByteArray.Length; i++)
                {
                    if (expectedByteArray[i] != actualByteArray[i])
                        return false;
                }

                return true;
            };

            return It.Is<Request>(req => isByteArray(req));
        }
    }

    public static class HanlderSetup
    {
        public static Response SetupResponse(this Mock<ICouchDBHandler> handler, object responseJsonAsObject)
        {
            return handler.SetupResponse(JsonConvert.SerializeObject(responseJsonAsObject));
        }

        public static Response SetupResponse(this Mock<ICouchDBHandler> handler, string responseJsonAsString)
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(responseJsonAsString);
            var response = new Response(httpResponseMessage);

            return handler.SetupResponse(response);
        }

        public static Response SetupResponse(this Mock<ICouchDBHandler> handler, byte[] responseArray)
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Content = new ByteArrayContent(responseArray);
            var response = new Response(httpResponseMessage);

            return handler.SetupResponse(response);
        }

        public static Response SetupResponse(this Mock<ICouchDBHandler> handler, Response response)
        {
            handler.Setup(h => h.SendRequestAsync(It.IsAny<string>(), It.IsAny<RequestMethod>(), It.IsAny<Request>()))
                .ReturnsAsync(response);

            return response;
        }

        public static void Throws(this Mock<ICouchDBHandler> handler, Exception exception)
        {
            handler.Setup(h => h.SendRequestAsync(It.IsAny<string>(), It.IsAny<RequestMethod>(), It.IsAny<Request>()))
                .ThrowsAsync(exception);
        }
    }
}
