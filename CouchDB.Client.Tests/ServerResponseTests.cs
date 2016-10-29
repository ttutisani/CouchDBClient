using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public class ServerResponseTests
    {
        [Fact]
        public void Ctor_Requires_DTO()
        {
            //act.
            Assert.Throws<ArgumentNullException>(() => new ServerResponse(null));
        }

        [Fact]
        public void Ctor_Initializes_PropertiesAsPassed()
        {
            //arrange.
            var serverResponseDTO = new CouchDBServer.ServerResponseDTO
            {
                OK = true,
                Error = ServerResponseError.File_Exists,
                Reason = "some reason here"
            };

            //act.
            var sut = new ServerResponse(serverResponseDTO);

            //assert.
            Assert.Equal(serverResponseDTO.OK, sut.OK);
            Assert.Equal(serverResponseDTO.Error, sut.Error);
            Assert.Equal(serverResponseDTO.Reason, sut.Reason);
        }
    }
}
