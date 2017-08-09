using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace CouchDB.Client.Tests
{
    public static class RequestIs
    {
        public static Request Empty()
        {
            return It.Is<Request>(req => req == Request.Empty);
        }
    }

    public static class HanlderSetup
    {
        public static Response SetupResponse(this Mock<ICouchDBHandler> handler, object responseJsonAsObject)
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(responseJsonAsObject));
            var response = new Response(httpResponseMessage);
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
