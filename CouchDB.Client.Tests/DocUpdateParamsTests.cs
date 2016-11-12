using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DocUpdateParamsTests
    {
        [Theory]
        [InlineData(true, "batch=ok")]
        [InlineData(false, "")]
        public void Batch_ComesIntoQuery_OnlyIfPresent(bool batch, string expected)
        {
            //arrange.
            var sut = new DocUpdateParams { Batch = batch };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal(expected, query);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData(true, "new_edits=true")]
        [InlineData(false, "new_edits=false")]
        public void NewEdits_ComesIntoQuery_AsBoolean(bool? newEdits, string expected)
        {
            //arrange.
            var sut = new DocUpdateParams { New_Edits = newEdits };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal(expected, query);
        }
    }
}
