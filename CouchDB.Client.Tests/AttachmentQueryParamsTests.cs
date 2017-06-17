using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class AttachmentQueryParamsTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "rev=")]
        [InlineData("rev123", "rev=rev123")]
        public void ToQueryString_GeneratesParams(string rev, string expectedQueryString)
        {
            //arrange.
            var sut = new AttachmentQueryParams { Rev = rev };

            //act.
            var actualQueryString = sut.ToQueryString();

            //assert.
            Assert.Equal(expectedQueryString, actualQueryString);
        }
    }
}
