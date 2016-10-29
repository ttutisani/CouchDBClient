using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public class CouchDBClientExceptionTests
    {
        [Fact]
        public void Ctor_Initializes_WithMessageOnly()
        {
            //arrange.
            var message = "some ex message";

            //act.
            var sut = new CouchDBClientException(message, null);

            //assert.
            Assert.Equal(message, sut.Message);
            Assert.Null(sut.InnerException);
        }

        [Fact]
        public void Ctor_Initializes_AllPropertiesAsPassed()
        {
            //arrange.
            var message = "some ex message";
            var serverResponse = new ServerResponse(new CouchDBServer.ServerResponseDTO { });
            var innerException = new Exception();

            //act.
            var sut = new CouchDBClientException(message, serverResponse, innerException);

            //assert.
            Assert.Equal(message, sut.Message);
            Assert.Same(serverResponse, sut.ServerResponse);
            Assert.Same(innerException, sut.InnerException);
        }
    }
}
