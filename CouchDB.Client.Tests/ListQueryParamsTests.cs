using System;
using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ListQueryParamsTests
    {
        [Fact]
        public void End_Key_IsAnAliasFor_EndKey()
        {
            //arrange / act.
            var sut = new ListQueryParams { End_Key = "end-key-123" };

            //assert.
            Assert.Equal(sut.End_Key, sut.EndKey);
        }

        [Fact]
        public void End_Key_Doc_Id_IsAnAliasFor_EndKey_DocId()
        {
            //arrange / act.
            var sut = new ListQueryParams { End_Key_Doc_Id = "end-key-doc-id-19218" };

            //assert.
            Assert.Equal(sut.End_Key_Doc_Id, sut.EndKey_DocId);
        }

        [Fact]
        public void Start_Key_IsAnAliasFor_StartKey()
        {
            //arrange / act.
            var sut = new ListQueryParams { Start_Key = "start-key-123" };

            //assert.
            Assert.Equal(sut.Start_Key, sut.StartKey);
        }

        [Fact]
        public void Start_Key_Doc_Id_IsAnAliasFor_StartKey_DocId()
        {
            //arrange / act.
            var sut = new ListQueryParams { Start_Key_Doc_Id = "start-key-doc-id-19218" };

            //assert.
            Assert.Equal(sut.Start_Key_Doc_Id, sut.StartKey_DocId);
        }

        [Fact]
        public void Alias_DoesNotAppearInQueryString()
        {
            //arrange.
            var sut = new ListQueryParams
            {
                End_Key = "end-key-123",
                End_Key_Doc_Id = "end-key-doc-id-19218",
                Start_Key = "start-key-123",
                Start_Key_Doc_Id = "start-key-doc-id-19218"
            };

            //act.
            var query = sut.ToQueryString();

            //assert.
            var queryParts = query.Split('&');
            Assert.Equal(4, queryParts.Length);
            Assert.True(queryParts.Any(p => p.Equals($"endkey=\"{sut.End_Key}\"", StringComparison.OrdinalIgnoreCase)));
            Assert.True(queryParts.Any(p => p.Equals($"endkey_docid=\"{sut.End_Key_Doc_Id}\"", StringComparison.OrdinalIgnoreCase)));
            Assert.True(queryParts.Any(p => p.Equals($"startkey=\"{sut.Start_Key}\"", StringComparison.OrdinalIgnoreCase)));
            Assert.True(queryParts.Any(p => p.Equals($"startkey_docid=\"{sut.Start_Key_Doc_Id}\"", StringComparison.OrdinalIgnoreCase)));
        }

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
