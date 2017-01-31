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

            //act.
            var sut = new ServerResponseError(errorString);

            //assert.
            Assert.Equal(errorString, sut.RawError);
            Assert.True(sut.CommonError.HasValue);
            Assert.Equal(expectedError, sut.CommonError);
        }

        [Fact]
        public void Ctor_Sets_Only_RawError_For_Unknown_Error()
        {
            //arrange.
            var errorString = "unknown_error_121212";

            //act.
            var sut = new ServerResponseError(errorString);

            //assert.
            Assert.Equal(errorString, sut.RawError);
            Assert.False(sut.CommonError.HasValue);
        }
    }
}
