using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ListQueryParamsTests
    {
        [Fact]
        public void NonChanged_Params_GiveNoQueryString()
        {
            //arrange.
            var sut = new ListQueryParams();

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal(string.Empty, query);
        }

        [Fact]
        public void BooleanParam_ComesWithoutQuotes()
        {
            //arrange.
            var sut = new ListQueryParams { Descending = true };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"descending={sut.Descending.Value}", query, ignoreCase: true);
        }

        [Fact]
        public void StringParam_ComesWithQuotes()
        {
            //arrange.
            var sut = new ListQueryParams { Key = "my key" };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"key=\"{sut.Key}\"", query, ignoreCase: true);
        }

        [Fact]
        public void Keys_ComeAsArrayOfStrings()
        {
            //arrange.
            var sut = new ListQueryParams { Keys = new[] { "key1", "key2" } };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"keys=[\"{sut.Keys.ElementAt(0)}\",\"{sut.Keys.ElementAt(1)}\"]", query, ignoreCase: true);
        }

        [Fact]
        public void Number_ComesWithoutQuotes()
        {
            //arrange.
            var sut = new ListQueryParams { Limit = 123 };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"limit={sut.Limit.Value}", query, ignoreCase: true);
        }

        [Fact]
        public void Stale_ComesWithoutQuotes()
        {
            //arrange.
            var sut = new ListQueryParams { Stale = ListQueryParams.StaleOption.Ok };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"stale={sut.Stale.Value.ToString()}", query, ignoreCase: true);
        }

        [Fact]
        public void QueryString_ContainsOnlySpecifiedValues()
        {
            //arrange.
            var sut = new ListQueryParams { Descending = true, Skip = 123, StartKey = "start-key" };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.NotNull(query);

            var parts = query.Split('&');
            Assert.Equal(3, parts.Length);

            Assert.True(parts.Any(p => p.Equals($"descending={sut.Descending.Value}", System.StringComparison.OrdinalIgnoreCase)));
            Assert.True(parts.Any(p => p.Equals($"skip={sut.Skip.Value}", System.StringComparison.OrdinalIgnoreCase)));
            Assert.True(parts.Any(p => p.Equals($"startkey=\"{sut.StartKey}\"", System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}
