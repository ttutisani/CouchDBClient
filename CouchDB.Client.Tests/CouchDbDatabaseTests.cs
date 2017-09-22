using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class CouchDbDatabaseTests
    {
        private readonly CouchDBDatabase _sut;
        private readonly Mock<ICouchDBHandler> _handler = new Mock<ICouchDBHandler>();

        public CouchDbDatabaseTests()
        {
            _sut = new CouchDBDatabase(_handler.Object);
        }

        [Fact]
        public void Ctor_Requires_Handler()
        {
            //arrange/act/assert.
            Assert.Throws<ArgumentNullException>(() => new CouchDBDatabase((ICouchDBHandler)null));
        }

        [Fact]
        public void Dispose_Clears_Disposable_Handler()
        {
            //arrange.
            var handler = new Mock<CouchDBServerTests.IDisposableHandler>();
            var sut = new CouchDBDatabase(handler.Object);

            //act.
            sut.Dispose();

            //assert.
            handler.Verify(h => h.Dispose(), Times.Once);
        }

        #region SaveDocumentAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SaveDocumentAsync_Requires_Params(string documentJson)
        {
            //act.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveDocumentAsync(documentJson));
        }

        [Theory]
        [InlineData("docjson", true, true)]
        [InlineData("docjson", true, false)]
        [InlineData("docjson", false, true)]
        [InlineData("docjson", false, false)]
        public async Task SaveDocumentAsync_Request(string documentJson, bool batch, bool newEdits)
        {
            //arrange.
            var docQueryParams = new DocUpdateParams { Batch = batch, New_Edits = newEdits };

            //act.
            await _sut.SaveDocumentAsync(documentJson, docQueryParams);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams(string.Empty, docQueryParams);
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.POST, RequestIs.JsonString(documentJson)));
        }

        [Fact]
        public async Task SaveDocumentAsync_Response()
        {
            //arrange.
            var expectedDTO = new CouchDBDatabase.SaveDocResponseDTO { Id = "some id", Rev = "some rev" };
            _handler.SetupResponse(expectedDTO);

            //act.
            var response = await _sut.SaveDocumentAsync("docjson-doesnotmatter");

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedDTO.Id, response.Id);
            Assert.Equal(expectedDTO.Rev, response.Revision);
            Assert.Null(response.Error);
        }

        [Fact]
        public async Task SaveDocumentAsync_Response_Error()
        {
            //arrange.
            var expectedDTO = new CouchDBDatabase.SaveDocResponseDTO { Error = "some error", Reason = "some reason" };
            _handler.SetupResponse(expectedDTO);

            //act.
            var response = await _sut.SaveDocumentAsync("docjson-doesnotmatter");

            //assert.
            Assert.NotNull(response);
            Assert.NotNull(response.Error);
            Assert.Equal(expectedDTO.Error, response.Error.RawError);
            Assert.Equal(expectedDTO.Reason, response.Error.Reason);
            Assert.Null(expectedDTO.Id);
            Assert.Null(expectedDTO.Rev);
        }

        #endregion

        #region GetDocumentAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetDocumentAsync_Requires_Params(string docId)
        {
            //act.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetDocumentAsync(docId));
        }

        [Theory]
        [InlineData("docid", true, new string[] { "1", "2" }, false, true, false, true, false, true,
            "123", true, false)]
        public async Task GetDocumentAsync_Request(
            string docId,
            bool? attachments = null,
            System.Collections.Generic.IEnumerable<string> attr_since = null,
            bool? attr_encoding_info = null,
            bool? conflicts = null,
            bool? deleted_conflicts = null,
            bool? latest = null,
            bool? local_seq = null,
            bool? meta = null,
            string rev = null,
            bool? revs = null,
            bool? revs_info = null)
        {
            //arrange.
            var docQueryParams = new DocQueryParams
            {
                Attachments = attachments,
                Atts_Since = attr_since,
                Att_Encoding_Info = attr_encoding_info,
                Conflicts = conflicts,
                Deleted_Conflicts = deleted_conflicts,
                Latest = latest,
                Local_Seq = local_seq,
                Meta = meta,
                Rev = rev,
                Revs = revs,
                Revs_Info = revs_info
            };

            //act.
            await _sut.GetDocumentAsync(docId, docQueryParams);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams(docId, docQueryParams);
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.GET, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentAsync_Response()
        {
            //arrange.
            var expectedDocument = "some-json-doc-string";
            _handler.SetupResponse(expectedDocument);

            //act.
            var document = await _sut.GetDocumentAsync("docid123");

            //assert.
            Assert.Equal(expectedDocument, document);
        }

        [Fact]
        public async Task GetDocumentAsync_NotFound_ResultsIntoNull()
        {
            //arrange.
            _handler.SetupResponse(new Response(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) { Content = new StringContent("string-doc") }));

            //act.
            var document = await _sut.GetDocumentAsync("docid");

            //assert.
            Assert.Null(document);
        }

        #endregion

        #region DeleteDocumentAsync

        [Theory]
        [InlineData(null, "good")]
        [InlineData("", "good")]
        [InlineData("   ", "good")]
        [InlineData("good", null)]
        [InlineData("good", "")]
        [InlineData("good", "   ")]
        public async Task DeleteDocumentAsync_Requires_Params(string docId, string revision)
        {
            //act.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteDocumentAsync(docId, revision));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DeleteDocumentAsync_Request(bool batch)
        {
            //arrange.
            var docId = "some doc id 123";
            var revision = "some revision 321";

            //act.
            await _sut.DeleteDocumentAsync(docId, revision, batch);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams(docId, new DeleteDocParams { Batch = batch, Revision = revision });
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.DELETE, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async Task DeleteDocumentAsync_Response()
        {
            //arrange.
            var expectedResponse = new CouchDBDatabase.SaveDocResponseDTO { Id = "1", Rev = "2", Reason = "3", Error = "4" };
            _handler.SetupResponse(expectedResponse);

            //act.
            var response = await _sut.DeleteDocumentAsync("doc id", "rev");

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Rev, response.Revision);
            Assert.NotNull(response.Error);
            Assert.Equal(expectedResponse.Reason, response.Error.Reason);
            Assert.Equal(expectedResponse.Error, response.Error.RawError);
        }

        [Fact]
        public async Task DeleteDocumentAsync_Response_NotFound_ResultsInSuccess()
        {
            //arrange.
            _handler.SetupResponse(new Response(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound }));
            var expectedDocId = "doc id 12askj";
            var expectedRevision = "rev asjajs";

            //act.
            var response = await _sut.DeleteDocumentAsync(expectedDocId, expectedRevision);

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedDocId, response.Id);
            Assert.Equal(expectedRevision, response.Revision);
            Assert.Null(response.Error);
        }

        #endregion

        #region GetAllDocumentsAsync

        [Theory]
        [InlineData(true, false, "endkey", null, true, false, "key", new string[0], 123, null, 
            ListQueryParams.StaleOption.Update_After, null, "startkeydocid", null)]
        public async Task GetAllDocumentsAsync_Request(
            bool? conflicts,
            bool? descending,
            string endKey,
            string endKey_DocId,
            bool? include_Docs,
            bool? inclusive_End,
            string key,
            IEnumerable<string> keys,
            int? limit,
            int? skip,
            ListQueryParams.StaleOption? stale,
            string startKey,
            string startKey_DocId,
            bool? update_Seq)
        {
            //arrange.
            var queryParams = new ListQueryParams
            {
                Conflicts = conflicts,
                Descending = descending,
                EndKey = endKey,
                EndKey_DocId = endKey_DocId,
                Include_Docs = include_Docs,
                Inclusive_End = inclusive_End,
                Key = key,
                Keys = keys,
                Limit = limit,
                Skip = skip,
                Stale = stale,
                StartKey = startKey,
                StartKey_DocId = startKey_DocId,
                Update_Seq = update_Seq
            };

            //act.
            await _sut.GetAllDocumentsAsync(queryParams);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.GET, RequestIs.Empty()));
        }

        [Fact]
        public async Task GetAllDocumentsAsync_Response()
        {
            //arrange.
            var expectedResponse = new
            {
                offset = 1,
                total_rows = 2,
                update_seq = 3,
                rows = new[] 
                {
                   new { id = "1", key = "2", value = new { rev = "3" }, error = "4", doc = new { test = 123, name = "123" } },
                   new { id = "2", key = "3", value = new { rev = "4" }, error = "5", doc = new { test = 234, name = "234" } },
                   new { id = "3", key = "4", value = new { rev = "5" }, error = "6", doc = new { test = 123, name = "345" } }
                }
            };
            _handler.SetupResponse(expectedResponse);

            //act.
            var allDocuments = await _sut.GetAllDocumentsAsync();

            //assert.
            Assert.NotNull(allDocuments);
            Assert.Equal(expectedResponse.offset, allDocuments.Offset);
            Assert.Equal(expectedResponse.total_rows, allDocuments.TotalRows);
            Assert.Equal(expectedResponse.update_seq, allDocuments.UpdateSeq);

            Assert.NotNull(allDocuments.Rows);
            Assert.Equal(3, allDocuments.Rows.Count);

            Assert.Equal(expectedResponse.rows[0].id, allDocuments.Rows[0].Id);
            Assert.Equal(expectedResponse.rows[0].key, allDocuments.Rows[0].Key);
            Assert.Equal(expectedResponse.rows[0].value.rev, allDocuments.Rows[0].Value.Revision);
            Assert.Equal(expectedResponse.rows[0].error, allDocuments.Rows[0].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(allDocuments.Rows[0].Document, expectedResponse.rows[0].doc));

            Assert.Equal(expectedResponse.rows[1].id, allDocuments.Rows[1].Id);
            Assert.Equal(expectedResponse.rows[1].key, allDocuments.Rows[1].Key);
            Assert.Equal(expectedResponse.rows[1].value.rev, allDocuments.Rows[1].Value.Revision);
            Assert.Equal(expectedResponse.rows[1].error, allDocuments.Rows[1].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(allDocuments.Rows[1].Document, expectedResponse.rows[1].doc));

            Assert.Equal(expectedResponse.rows[2].id, allDocuments.Rows[2].Id);
            Assert.Equal(expectedResponse.rows[2].key, allDocuments.Rows[2].Key);
            Assert.Equal(expectedResponse.rows[2].value.rev, allDocuments.Rows[2].Value.Revision);
            Assert.Equal(expectedResponse.rows[2].error, allDocuments.Rows[2].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(allDocuments.Rows[2].Document, expectedResponse.rows[2].doc));
        }

        #endregion

        #region GetDocumentsAsync

        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new string[0] })]
        public async Task GetDocumentsAsync_RequiresParams(string[] docIdList)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetDocumentsAsync(docIdList));
        }

        [Theory]
        [InlineData(true, false, "endkey", null, true, false, "key", new string[0], 123, null,
            ListQueryParams.StaleOption.Update_After, null, "startkeydocid", null)]
        public async Task GetDocumentsAsync_Request(
            bool? conflicts,
            bool? descending,
            string endKey,
            string endKey_DocId,
            bool? include_Docs,
            bool? inclusive_End,
            string key,
            IEnumerable<string> keys,
            int? limit,
            int? skip,
            ListQueryParams.StaleOption? stale,
            string startKey,
            string startKey_DocId,
            bool? update_Seq)
        {
            //arrange.
            var docIdList = new[] { "1", "2", "3" };

            var queryParams = new ListQueryParams
            {
                Conflicts = conflicts,
                Descending = descending,
                EndKey = endKey,
                EndKey_DocId = endKey_DocId,
                Include_Docs = include_Docs,
                Inclusive_End = inclusive_End,
                Key = key,
                Keys = keys,
                Limit = limit,
                Skip = skip,
                Stale = stale,
                StartKey = startKey,
                StartKey_DocId = startKey_DocId,
                Update_Seq = update_Seq
            };

            //act.
            await _sut.GetDocumentsAsync(docIdList, queryParams);

            //assrert.
            var expectedUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.POST, 
                RequestIs.JsonObject(new { keys = docIdList })), Times.Once);
        }

        [Fact]
        public async Task GetDocumentsAsync_Response()
        {
            //arrange.
            var expectedResponse = new
            {
                offset = 1,
                total_rows = 2,
                update_seq = 3,
                rows = new[]
                {
                   new { id = "1", key = "2", value = new { rev = "3" }, error = "4", doc = new { test = 123, name = "123" } },
                   new { id = "2", key = "3", value = new { rev = "4" }, error = "5", doc = new { test = 234, name = "234" } },
                   new { id = "3", key = "4", value = new { rev = "5" }, error = "6", doc = new { test = 123, name = "345" } }
                }
            };
            _handler.SetupResponse(expectedResponse);

            //act.
            var docs = await _sut.GetDocumentsAsync(new[] { "id1" });

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(expectedResponse.offset, docs.Offset);
            Assert.Equal(expectedResponse.total_rows, docs.TotalRows);
            Assert.Equal(expectedResponse.update_seq, docs.UpdateSeq);

            Assert.NotNull(docs.Rows);
            Assert.Equal(3, docs.Rows.Count);

            Assert.Equal(expectedResponse.rows[0].id, docs.Rows[0].Id);
            Assert.Equal(expectedResponse.rows[0].key, docs.Rows[0].Key);
            Assert.Equal(expectedResponse.rows[0].value.rev, docs.Rows[0].Value.Revision);
            Assert.Equal(expectedResponse.rows[0].error, docs.Rows[0].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(docs.Rows[0].Document, expectedResponse.rows[0].doc));

            Assert.Equal(expectedResponse.rows[1].id, docs.Rows[1].Id);
            Assert.Equal(expectedResponse.rows[1].key, docs.Rows[1].Key);
            Assert.Equal(expectedResponse.rows[1].value.rev, docs.Rows[1].Value.Revision);
            Assert.Equal(expectedResponse.rows[1].error, docs.Rows[1].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(docs.Rows[1].Document, expectedResponse.rows[1].doc));

            Assert.Equal(expectedResponse.rows[2].id, docs.Rows[2].Id);
            Assert.Equal(expectedResponse.rows[2].key, docs.Rows[2].Key);
            Assert.Equal(expectedResponse.rows[2].value.rev, docs.Rows[2].Value.Revision);
            Assert.Equal(expectedResponse.rows[2].error, docs.Rows[2].Error.RawError);
            Assert.True(AssertHelper.StringIsJsonObject(docs.Rows[2].Document, expectedResponse.rows[2].doc));
        }

        #endregion

        #region SaveDocumentsAsync

        [Theory]
        [InlineData(null)]
        [InlineData(new object[] { new string[0] })]
        public async Task SaveDocumentsAsync_Requires_Params(string[] documents)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveDocumentsAsync(documents));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveDocumentsAsync_Request(bool newEdits)
        {
            //arrange.
            var expectedObject = new
            {
                new_edits = newEdits,
                docs = new [] 
                {
                    new { id = "1", name = 2 },
                    new { id = "2", name = 4 }
                }
            };

            //act.
            await _sut.SaveDocumentsAsync(expectedObject.docs.Select(d => JsonConvert.SerializeObject(d)).ToArray(), newEdits);

            //assert.
            _handler.Verify(h => h.SendRequestAsync("_bulk_docs", RequestMethod.POST, RequestIs.JsonObject(expectedObject)), Times.Once);
        }

        [Fact]
        public async Task SaveDocumentsAsync_Response()
        {
            //arrange.
            var expectedDTO = new CouchDBDatabase.SaveDocListResponseDTO
            {
                new CouchDBDatabase.SaveDocResponseDTO { Id = "1", Rev = "2", Error = "3", Reason = "4" },
                new CouchDBDatabase.SaveDocResponseDTO { Id = "5", Rev = "6", Error = "7", Reason = "7" },
            };

            _handler.SetupResponse(expectedDTO);

            //act.
            var response = await _sut.SaveDocumentsAsync(new string[] { "{}" });

            //assert.
            Assert.NotNull(response);
            Assert.NotNull(response.DocumentResponses);
            Assert.Equal(expectedDTO.Count, response.DocumentResponses.Count);

            Assert.Equal(expectedDTO[0].Id, response.DocumentResponses[0].Id);
            Assert.Equal(expectedDTO[0].Rev, response.DocumentResponses[0].Revision);
            Assert.NotNull(response.DocumentResponses[0].Error);
            Assert.Equal(expectedDTO[0].Error, response.DocumentResponses[0].Error.RawError);
            Assert.Equal(expectedDTO[0].Reason, response.DocumentResponses[0].Error.Reason);

            Assert.Equal(expectedDTO[1].Id, response.DocumentResponses[1].Id);
            Assert.Equal(expectedDTO[1].Rev, response.DocumentResponses[1].Revision);
            Assert.NotNull(response.DocumentResponses[1].Error);
            Assert.Equal(expectedDTO[1].Error, response.DocumentResponses[1].Error.RawError);
            Assert.Equal(expectedDTO[1].Reason, response.DocumentResponses[1].Error.Reason);
        }

        #endregion

        #region SaveAttachmentAsync

        [Theory]
        [InlineData(null, "good", "good", new byte[0])]
        [InlineData("", "good", "good", new byte[0])]
        [InlineData("   ", "good", "good", new byte[0])]
        [InlineData("good", null, "good", new byte[0])]
        [InlineData("good", "", "good", new byte[0])]
        [InlineData("good", "   ", "good", new byte[0])]
        [InlineData("good", "good", null, new byte[0])]
        [InlineData("good", "good", "", new byte[0])]
        [InlineData("good", "good", "   ", new byte[0])]
        [InlineData("good", "good", "good", null)]
        public async Task SaveAttachmentAsync_Requires_Arguments(string docId, string attName, string revision, byte[] attachment)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAttachmentAsync(docId, attName, revision, attachment));
        }

        [Fact]
        public async Task SaveAttachmentAsync_Request()
        {
            //arrange.
            string docId = "123", attName = "234", revision = "345";
            var attachment = new byte[] { 1, 3, 2 };

            //act.
            await _sut.SaveAttachmentAsync(docId, attName, revision, attachment);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams($"{docId}/{attName}", new AttachmentQueryParams { Rev = revision });
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.PUT, RequestIs.ByteArray(attachment)), Times.Once);
        }

        [Fact]
        public async Task SaveAttachmentAsync_Response()
        {
            //arrange.
            var expectedResponse = new CouchDBDatabase.SaveDocResponseDTO
            {
                Id = "1",
                Rev = "2",
                Error = "3",
                Reason = "4"
            };
            _handler.SetupResponse(expectedResponse);

            //act.
            var response = await _sut.SaveAttachmentAsync("docid", "attname", "revision", new byte[0]);

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedResponse.Id, response.Id);
            Assert.Equal(expectedResponse.Rev, response.Revision);
            Assert.NotNull(response.Error);
            Assert.Equal(expectedResponse.Error, response.Error.RawError);
            Assert.Equal(expectedResponse.Reason, response.Error.Reason);
        }

        #endregion

        #region GetAttachmentAsync

        [Theory]
        [InlineData(null, "good")]
        [InlineData("", "good")]
        [InlineData("   ", "good")]
        [InlineData("good", null)]
        [InlineData("good", "")]
        [InlineData("good", "   ")]
        public async Task GetAttachmentAsync_Requires_Params(string docId, string attName)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAttachmentAsync(docId, attName));
        }

        [Fact]
        public async Task GetAttachmentAsync_Request()
        {
            //arrange.
            string docId = "1", attName = "2";

            //act.
            await _sut.GetAttachmentAsync(docId, attName);

            //assert.
            var expectedUrl = $"{docId}/{attName}";
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.GET, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async Task GetAttachmentAsync_Response()
        {
            //arrange.
            var expectedAttachment = new byte[] { 1, 2, 3 };
            _handler.SetupResponse(expectedAttachment);

            //act.
            var response = await _sut.GetAttachmentAsync("docid", "attname");

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedAttachment, response);
        }

        #endregion

        #region DeleteAttachmentAsync

        [Theory]
        [InlineData(null, "good", "good")]
        [InlineData("", "good", "good")]
        [InlineData("   ", "good", "good")]
        [InlineData("good", null, "good")]
        [InlineData("good", "", "good")]
        [InlineData("good", "   ", "good")]
        [InlineData("good", "good", null)]
        [InlineData("good", "good", "")]
        [InlineData("good", "good", "   ")]
        public async Task DeleteAttachmentAsync_Requires_Params(string docId, string attName, string revision)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteAttachmentAsync(docId, attName, revision));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteAttachmentAsync_Request(bool batch)
        {
            //arrange.
            var docId = "docid-123";
            var attName = "attName-234";
            var revision = "rev-543";

            //act.
            await _sut.DeleteAttachmentAsync(docId, attName, revision, batch);

            //assert.
            var expectedUrl = QueryParams.AppendQueryParams($"{docId}/{attName}", new DeleteDocParams { Revision = revision, Batch = batch });
            _handler.Verify(h => h.SendRequestAsync(expectedUrl, RequestMethod.DELETE, RequestIs.Empty()), Times.Once);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Response()
        {
            //arrange.
            var expectedDTO = new CouchDBDatabase.SaveDocResponseDTO { Id = "1", Rev = "2", Error = "3", Reason = "4" };
            _handler.SetupResponse(expectedDTO);

            //act.
            var response = await _sut.DeleteAttachmentAsync("docid", "attname", "revision");

            //assert.
            Assert.NotNull(response);
            Assert.Equal(expectedDTO.Id, response.Id);
            Assert.Equal(expectedDTO.Rev, response.Revision);
            Assert.NotNull(response.Error);
            Assert.Equal(expectedDTO.Error, response.Error.RawError);
            Assert.Equal(expectedDTO.Reason, response.Error.Reason);
        }

        #endregion
    }
}
