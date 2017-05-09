using Moq;
using System;
using System.Runtime.Serialization;
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

        [Fact]
        public void GetObjectData_Sets_Info_About_Members()
        {
            //arrange.
            var serverResponse = new ServerResponse(new CouchDBServer.ServerResponseDTO());
            var sut = new CouchDBClientException("message does not matter", serverResponse);

            //act.
            var serializationInfo = new SerializationInfo(typeof(CouchDBClientException), new Mock<IFormatterConverter>().Object);
            sut.GetObjectData(serializationInfo, new StreamingContext());

            //assert.
            Assert.Same(serverResponse, serializationInfo.GetValue(CouchDBClientException.ServerResponse_Key_InSerializationInfo, typeof(ServerResponse)));
        }
    }
}
