using System;
using System.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class QueryParamsTests
    {
        [Fact]
        public void AppendQueryParams_RequiresUrl()
        {
            //arrange.
            string url = null;

            //act / assert.
            Assert.Throws<ArgumentNullException>(() => QueryParams.AppendQueryParams(url, new ListQueryParams { }));
        }

        [Fact]
        public void AppendQueryParams_DoesNotAppend_IfQueryParamsIsNull()
        {
            //arrange.
            var url = "url";

            //act.
            var newUrl = QueryParams.AppendQueryParams(url, null);

            //assert.
            Assert.Equal(url, newUrl);
        }

        [Fact]
        public void AppendQueryParams_DoesNotAppend_IfQueryParamsIsEmpty()
        {
            //arrange.
            var url = "url";
            var queryParams = new ListQueryParams { };

            //act.
            var newUrl = QueryParams.AppendQueryParams(url, queryParams);

            //assert.
            Assert.Equal(url, newUrl);
        }

        [Fact]
        public void AppendQueryParams_AppendsQueryWithQMark_IfNoneYet()
        {
            //arrange.
            var url = "url";
            var queryParams = new ListQueryParams { EndKey = "end-key" };

            //act.
            var newUrl = QueryParams.AppendQueryParams(url, queryParams);

            //assert.
            Assert.Equal($"{url}?{queryParams.ToQueryString()}", newUrl);
        }

        [Fact]
        public void AppendQueryParam_AppendsQueryWithoutQMark_IfQMarkPresent()
        {
            //arrange.
            var url = "url?param1=value1";
            var queryParams = new ListQueryParams { EndKey = "end-key" };

            //act.
            var newUrl = QueryParams.AppendQueryParams(url, queryParams);

            //assert.
            Assert.Equal($"{url}&{queryParams.ToQueryString()}", newUrl);
        }

        [Fact]
        public void AppendQueryParams_AddsQueryWithoutQMark_IfQMarkAtTheEnd()
        {
            //arrange.
            var url = "url?";
            var queryParams = new ListQueryParams { EndKey = "end-key" };

            //act.
            var newUrl = QueryParams.AppendQueryParams(url, queryParams);

            //assert.
            Assert.Equal($"{url}{queryParams.ToQueryString()}", newUrl);
        }

        [Fact]
        public void FromListParams_Null_GivesNull()
        {
            //arrange.
            ListQueryParams source = null;

            //act.
            QueryParams sut = source;

            //assert.
            Assert.Null(sut);
        }

        [Fact]
        public void FromListParams_GivesSameQueryString()
        {
            //arrange.
            var listParams = new ListQueryParams { Conflicts = false, StartKey = "start-key" };

            //act.
            QueryParams sut = listParams;

            //assert.
            Assert.NotNull(sut);
            Assert.Equal(listParams.ToQueryString(), sut.ToQueryString());
        }

        [Fact]
        public void FromDocParams_Null_GivesNull()
        {
            //arrange.
            DocQueryParams docParams = null;

            //act.
            QueryParams sut = docParams;

            //assert.
            Assert.Null(sut);
        }

        [Fact]
        public void FromDocParams_GivesSameQueryString()
        {
            //arrange.
            var docParams = new DocQueryParams { Conflicts = false, Rev = "revision here 127612" };

            //act.
            QueryParams sut = docParams;

            //assert.
            Assert.NotNull(sut);
            Assert.Equal(docParams.ToQueryString(), sut.ToQueryString());
        }
    }
}
