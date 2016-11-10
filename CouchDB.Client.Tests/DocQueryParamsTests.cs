using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DocQueryParamsTests
    {
        [Fact]
        public void NonChanged_Params_GiveNoQueryString()
        {
            //arrange.
            var sut = new DocQueryParams();

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal(string.Empty, query);
        }

        [Fact]
        public void BooleanParam_ComesWithoutQuotes()
        {
            //arrange.
            var sut = new DocQueryParams { Conflicts = true };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"conflicts={sut.Conflicts.Value}", query, ignoreCase: true);
        }

        [Fact]
        public void StringParam_ComesWithoutQuotes()
        {
            //arrange.
            var sut = new DocQueryParams { Rev = "rev-123" };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"rev={sut.Rev}", query, ignoreCase: true);
        }

        [Fact]
        public void AttsSince_ComeAsArrayOfStrings()
        {
            //arrange.
            var sut = new DocQueryParams { Atts_Since = new[] { "key1", "key2" } };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"atts_since=[\"{sut.Atts_Since.ElementAt(0)}\",\"{sut.Atts_Since.ElementAt(1)}\"]", query, ignoreCase: true);
        }

        [Fact]
        public void OpenRevs_ComesAsAll_WhenAssignedWithoutRevsArray()
        {
            //arrange.
            var sut = new DocQueryParams { Open_Revs = new DocQueryParams.OpenRevs() };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"open_revs=all", query, ignoreCase: true);
        }

        [Fact]
        public void OpenRevs_ComesAsArrayOfStrings_IfSpecified()
        {
            //arrange.
            var sut = new DocQueryParams { Open_Revs = new DocQueryParams.OpenRevs(new[] { "rev-1", "rev-2" }) };

            //act.
            var query = sut.ToQueryString();

            //assert.
            Assert.Equal($"open_revs=[\"rev-1\",\"rev-2\"]", query, ignoreCase: true);
        }
    }
}
