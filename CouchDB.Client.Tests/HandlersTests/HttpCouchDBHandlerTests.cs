using Moq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.Tests
{
    public class HttpCouchDBHandlerTests
    {
        internal virtual HttpCouchDBHandler CreateSut(string baseUrl, IStatelessHttpClient httpClient)
        {
            return new HttpCouchDBHandler(baseUrl, httpClient);
        }

        internal virtual string CreateInitialBaseUrl()
        {
            return "http://validurl";
        }

        internal virtual string CreateExpectedBaseUrl()
        {
            return "http://validurl";
        }

        public static object[] DataFor_Ctor_Requires_Arguments
        {
            get
            {
                return new Func<HttpCouchDBHandler>[] 
                {
                    () => new HttpCouchDBHandler(null, null),
                    () => new HttpCouchDBHandler(string.Empty, new Mock<IStatelessHttpClient>().Object),
                    () => new HttpCouchDBHandler("   ", new Mock<IStatelessHttpClient>().Object),
                    () => new HttpCouchDBHandler("http://validurl", null)
                }
                .Select(func => new object[] { func })
                .ToArray();
            }
        }

        [Theory]
        [MemberData(nameof(DataFor_Ctor_Requires_Arguments))]
        public virtual void Ctor_Requires_Arguments(Func<ICouchDBHandler> sutFactory)
        {
            //arrange / act / assert.
            Assert.Throws<ArgumentNullException>(sutFactory);
        }

        public interface IDisposableHttpClient : IStatelessHttpClient, IDisposable
        {

        }

        [Fact]
        public void Dispose_Clears_HttpClient()
        {
            //arrange.
            var httpClient = new Mock<IDisposableHttpClient>();
            var sut = CreateSut(CreateInitialBaseUrl(), httpClient.Object);

            //act.
            sut.Dispose();

            //assert.
            httpClient.Verify(c => c.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(RequestMethod.GET)]
        [InlineData(RequestMethod.POST)]
        [InlineData(RequestMethod.PUT)]
        [InlineData(RequestMethod.DELETE)]
        public async Task SendRequestAsync_Sends_AbsoluteUrl_For_Get(RequestMethod requestMethod)
        {
            //arrange.
            var baseUrl = CreateInitialBaseUrl();
            var relativeUrl = "123";
            var expectedRequest = Request.Raw(new byte[] { });

            var httpClient = new Mock<IStatelessHttpClient>();
            httpClient.Setup(c => c.SendRequestAsync(It.IsAny<string>(), It.IsAny<RequestMethod>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Response(new HttpResponseMessage(System.Net.HttpStatusCode.OK))));

            var sut = CreateSut(baseUrl, httpClient.Object);

            //act.
            await sut.SendRequestAsync(relativeUrl, requestMethod, expectedRequest);

            //assert.
            httpClient.Verify(c => c.SendRequestAsync($"{CreateExpectedBaseUrl()}/{relativeUrl}", requestMethod, expectedRequest), Times.Once);
        }
    }
}
