using System.Net.Http;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ResponseTests
    {
        private readonly HttpResponseMessage _message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        private readonly Response _sut;

        public ResponseTests()
        {
            _sut = new Response(_message);
        }

        [Fact]
        public void GetHttpResponseMessage_Retrieves_Originally_Wrapped_Message()
        {
            //act.
            var message = _sut.GetHttpResponseMessage();

            //assert.
            Assert.Same(_message, message);
        }
    }
}
