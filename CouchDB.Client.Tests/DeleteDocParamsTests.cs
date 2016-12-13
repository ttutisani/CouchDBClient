using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DeleteDocParamsTests
    {
        [Theory]
        [InlineData(null, false, "")]
        [InlineData("some-rev", false, "rev=some-rev")]
        [InlineData(null, true, "batch=ok")]
        [InlineData("myrevisionnumber", true, "rev=myrevisionnumber&batch=ok")]
        public void ToQueryString_DoesNotAddAnything_IfEmpty(string rev, bool batch, string expected)
        {
            //arrange.
            var sut = new DeleteDocParams
            {
                Revision = rev,
                Batch = batch
            };

            //act.
            var url = sut.ToQueryString();

            //assert.
            Assert.Equal(expected, url);
        }
    }
}
