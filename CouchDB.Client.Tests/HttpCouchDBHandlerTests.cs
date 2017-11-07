using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class HttpCouchDBHandlerTests
    {
        public static object[] DataFor_Ctor_Requires_Arguments
        {
            get
            {
                return new object[] 
                {
                    new object[] { null, null },
                    new object[] { string.Empty, new Mock<IStatelessHttpClient>().Object },
                    new object[] { "   ", new Mock<IStatelessHttpClient>().Object },
                    new object[] { "http://validurl", null }
                };
            }
        }

        [Theory]
        [MemberData(nameof(DataFor_Ctor_Requires_Arguments))]
        public void Ctor_Requires_Arguments(string baseUrl, IStatelessHttpClient http)
        {
            //arrange / act / assert.
            Assert.Throws<ArgumentNullException>(() => new HttpCouchDBHandler(baseUrl, http));
        }

        public interface IDisposableHttpClient : IStatelessHttpClient, IDisposable
        {

        }

        [Fact]
        public void Dispose_Clears_HttpClient()
        {
            //arrange.
            var httpClinet = new Mock<IDisposableHttpClient>();
            var sut = new HttpCouchDBHandler("http://validurl", httpClinet.Object);

            //act.
            sut.Dispose();

            //assert.
            httpClinet.Verify(c => c.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(RequestMethod.GET)]
        [InlineData(RequestMethod.POST)]
        [InlineData(RequestMethod.PUT)]
        [InlineData(RequestMethod.DELETE)]
        public async Task SendRequestAsync_Sends_AbsoluteUrl_For_Get(RequestMethod requestMethod)
        {
            //arrange.
            var baseUrl = "http://validurl";
            var relativeUrl = "123";
            var expectedRequest = Request.Raw(new byte[] { });

            var httpClient = new Mock<IStatelessHttpClient>();
            httpClient.Setup(c => c.SendRequestAsync(It.IsAny<string>(), It.IsAny<RequestMethod>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Response(new HttpResponseMessage(System.Net.HttpStatusCode.OK))));

            var sut = new HttpCouchDBHandler(baseUrl, httpClient.Object);

            //act.
            await sut.SendRequestAsync(relativeUrl, requestMethod, expectedRequest);

            //assert.
            httpClient.Verify(c => c.SendRequestAsync($"{baseUrl}/{relativeUrl}", requestMethod, expectedRequest), Times.Once);
        }
    }
}
