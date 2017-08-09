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
        public void Ctor_Assigns_NullError_IfNotPassed()
        {
            //arrange.
            var serverResponseDTO = new ServerResponseDTO
            {
                OK = true,
                Error = null,
                Reason = "some reason here"
            };

            //act.
            var sut = new ServerResponse(serverResponseDTO);

            //assert.
            Assert.Null(sut.Error);
        }

        [Fact]
        public void Ctor_Initializes_PropertiesAsPassed()
        {
            //arrange.
            var serverResponseDTO = new ServerResponseDTO
            {
                OK = true,
                Error = CommonError.File_Exists.ToErrorString(),
                Reason = "some reason here"
            };

            //act.
            var sut = new ServerResponse(serverResponseDTO);

            //assert.
            Assert.Equal(serverResponseDTO.OK, sut.OK);
            Assert.NotNull(sut.Error);
            Assert.True(sut.Error.CommonError.HasValue);
            Assert.True(sut.Error.CommonError.GetValueOrDefault().EqualsErrorString(serverResponseDTO.Error));
            Assert.Equal(serverResponseDTO.Reason, sut.Reason);
        }
    }
}
