using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class CouchDBServerTests
    {
        [Fact]
        public void Ctor_Requires_Parameters()
        {
            Assert.Throws<ArgumentNullException>(() => new CouchDBServer((ICouchDBHandler)null));
        }

        public interface IDisposableHandler : ICouchDBHandler, IDisposable
        { }
        
        private readonly CouchDBServer _sut;
        private readonly Mock<ICouchDBHandler> _handler = new Mock<ICouchDBHandler>();

        public CouchDBServerTests()
        {
            _sut = new CouchDBServer(_handler.Object);
        }

        #region GetInfoAsync

        [Fact]
        public async void GetInfoAsync_Request()
        {
            //arrange.

            //act.
            await _sut.GetInfoAsync();

            //assert.
            _handler.Verify(h => h.SendRequestAsync(string.Empty, RequestMethod.GET, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async void GetInfoAsync_Response()
        {
            //arrange.
            var expectedServerInfo = new CouchDBServer.ServerInfoDTO { CouchDB = "123", Version = "234", Vendor = new CouchDBServer.ServerInfoDTO.VendorInfoDTO { Name = "345" } };

            var response = _handler.SetupResponse(expectedServerInfo);
            
            //act.
            var info = await _sut.GetInfoAsync();

            //assert.
            Assert.NotNull(info);
            Assert.Equal(expectedServerInfo.CouchDB, info.CouchDB);
            Assert.Equal(expectedServerInfo.Version, info.Version);
            Assert.NotNull(info.Vendor);
            Assert.Equal(expectedServerInfo.Vendor.Name, info.Vendor.Name);
        }

        #endregion

        #region GetAllDbNamesAsync

        [Theory]
        [InlineData(true, false, "endkey", "endkey_docsid", false, true, "key", new[] { "key1, key2" }, 1, 2, ListQueryParams.StaleOption.Update_After, "startkey", "startkey_docsid", true)]
        public async void GetAllDbNamesAsync_Request(
            bool? conflicts, 
            bool? descending, 
            string endKey, 
            string endkey_docid, 
            bool? include_docs,
            bool? inclusive_end,
            string key,
            IEnumerable<string> keys,
            int? limit,
            int? skip,
            ListQueryParams.StaleOption? stale,
            string startKey,
            string startkey_docid,
            bool? update_seq
            )
        {
            //arrange.
            var queryParams = new ListQueryParams
            {
                Conflicts = conflicts, Descending = descending, EndKey = endKey, EndKey_DocId = endkey_docid,
                Include_Docs = include_docs, Inclusive_End = inclusive_end, Key = key, Keys = keys,
                Limit = limit, Skip = skip, Stale = stale, StartKey = startKey, StartKey_DocId = startkey_docid,
                Update_Seq = update_seq
            };

            //act.
            await _sut.GetAllDbNamesAsync(queryParams);

            //assert.
            var expectedRelativeUrl = QueryParams.AppendQueryParams("_all_dbs", queryParams);
            _handler.Verify(h => h.SendRequestAsync(expectedRelativeUrl, RequestMethod.GET, RequestIs.Empty()), Times.Once);
        }

        [Theory]
        [InlineData(new object[] { new string [] { } })]
        [InlineData(new object[] { new [] { "db1" } })]
        [InlineData(new object[] { new [] { "db1", "db2" } })]
        [InlineData(new object[] { new [] { "db1", "db2", "db3", "db4" } })]
        public async void GetAllDbNamesAsync_Response(string[] expectedDbNames)
        {
            //arrange.
            var response = _handler.SetupResponse(expectedDbNames);
            
            //act.
            var dbNames = await _sut.GetAllDbNamesAsync();

            //assert.
            Assert.NotNull(dbNames);
            Assert.Equal(expectedDbNames, dbNames);
        }

        #endregion

        #region CreateDbAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public async void CreateDbAsync_Requires_Params(string dbName)
        {
            //act.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateDbAsync(dbName));
        }

        [Fact]
        public async void CreateDbAsync_Request()
        {
            //arrange.
            var expectedDbName = "some-db-name";

            //act.
            await _sut.CreateDbAsync(expectedDbName);

            //assert.
            _handler.Verify(h => h.SendRequestAsync(expectedDbName, RequestMethod.PUT, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async void CreateDbAsync_Response()
        {
            //arrange.
            _handler.Throws(new CouchDBClientException("msg"));

            //act / assert.
            await Assert.ThrowsAsync<CouchDBClientException>(async () => await _sut.CreateDbAsync("db-name"));
        }

        #endregion

        #region DeleteDbAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async void DeleteDbAsync_Requires_Params(string dbName)
        {
            //assert.
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.DeleteDbAsync(dbName));
        }

        [Fact]
        public async void DeleteDbAsync_Request()
        {
            //arrange.
            var expectedDbName = "some-db-name-here";

            //act.
            await _sut.DeleteDbAsync(expectedDbName);

            //assert.
            _handler.Verify(h => h.SendRequestAsync(expectedDbName, RequestMethod.DELETE, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async Task DeleteDbAsync_Response()
        {
            //arrange.
            _handler.Throws(new CouchDBClientException("msg"));

            //act / assert.
            await Assert.ThrowsAsync<CouchDBClientException>(async () => await _sut.DeleteDbAsync("db-name"));
        }

        #endregion

        [Fact]
        public void GetHandler_Retrieves_Originally_Injected_Handler()
        {
            //act.
            var handler = _sut.GetHandler();

            //assert.
            Assert.Same(_handler.Object, handler);
        }
    }
}
