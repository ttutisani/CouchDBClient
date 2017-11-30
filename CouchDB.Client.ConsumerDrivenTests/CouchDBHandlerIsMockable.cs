using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.ConsumerDrivenTests
{
    public sealed class CouchDBHandlerIsMockable
    {
        private readonly Mock<ICouchDBHandler> _sut = new Mock<ICouchDBHandler>();

        [Fact]
        public void SendRequestAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SendRequestAsync(It.IsAny<string>(), It.IsAny<RequestMethod>(), It.IsAny<Request>()))
                .Returns(Task.FromResult(new Response(new HttpResponseMessage(System.Net.HttpStatusCode.OK))));
        }
    }
}
