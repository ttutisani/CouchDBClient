using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ListQueryParamsTests
    {
        [Fact]
        public void Include_Docs_Default_Value_Is_True()
        {
            //arrange / act.
            var sut = new ListQueryParams();

            //assert.
            Assert.True(sut.Include_Docs.HasValue);
            Assert.Equal(true, sut.Include_Docs.Value);
        }

        [Fact]
        public void Empty_Params_GiveNoQueryString()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal(string.Empty, query);
        }

        [Fact]
        public void BooleanParam_ComesWithoutQuotes()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Descending = true;

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"descending={sut.Descending.Value}", query, ignoreCase: true);
        }

        [Fact]
        public void StringParam_ComesWithQuotes()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Key = "my key";

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"key=\"{sut.Key}\"", query, ignoreCase: true);
        }

        [Fact]
        public void Keys_ComeAsArrayOfStrings()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Keys = new[] { "key1", "key2" };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"keys=[\"{sut.Keys.ElementAt(0)}\",\"{sut.Keys.ElementAt(1)}\"]", query, ignoreCase: true);
        }

        [Fact]
        public void Number_ComesWithoutQuotes()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Limit = 123;

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"limit={sut.Limit.Value}", query, ignoreCase: true);
        }

        [Fact]
        public void Stale_ComesWithoutQuotes()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Stale = ListQueryParams.StaleOption.Ok;

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"stale={sut.Stale.Value.ToString()}", query, ignoreCase: true);
        }

        [Fact]
        public void QueryString_ContainsOnlySpecifiedValues()
        {
            //arrange.
            var sut = ListQueryParams.CreateEmpty();
            sut.Descending = true;
            sut.Skip = 123;
            sut.StartKey = "start-key";

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
