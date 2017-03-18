using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ServerResponseErrorTests
    {
        [Fact]
        public void Ctor_Requires_Not_Null_Error()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerResponseError(null));
        }

        [Fact]
        public void Ctor_Sets_All_Properties_For_Known_Common_Error()
        {
            //arrange.
            var expectedError = CommonError.Illegal_DocId;
            var errorString = expectedError.ToErrorString();
            var expectedReason = "some reason 123";

            //act.
            var sut = new ServerResponseError(errorString, expectedReason);

            //assert.
            Assert.Equal(errorString, sut.RawError);
            Assert.True(sut.CommonError.HasValue);
            Assert.Equal(expectedError, sut.CommonError);
            Assert.Equal(expectedReason, sut.Reason);
        }

        [Fact]
        public void Ctor_Sets_Only_RawError_For_Unknown_Error()
        {
            //arrange.
            var errorString = "unknown_error_121212";
            var reason = "reason something 123";

            //act.
            var sut = new ServerResponseError(errorString, reason);

            //assert.
            Assert.Equal(errorString, sut.RawError);
            Assert.False(sut.CommonError.HasValue);
            Assert.Equal(reason, sut.Reason);
        }

        [Fact]
        public void FromString_Returns_Null_For_Null_ErrorString()
        {
            //act.
            var sut = ServerResponseError.FromString(null, "reason does not matter");

            //assert.
            Assert.Null(sut);
        }

        [Fact]
        public void FromString_Constructs_SUT_For_NotNull_ErrorString()
        {
            //arrange.
            var expectedError = "some error ajs";
            var expectedReason = "some reason asjh";

            //act.
            var sut = ServerResponseError.FromString(expectedError, expectedReason);

            //assert.
            Assert.NotNull(sut);
            Assert.Equal(expectedError, sut.RawError);
            Assert.Equal(expectedReason, sut.Reason);
        }
    }
}
