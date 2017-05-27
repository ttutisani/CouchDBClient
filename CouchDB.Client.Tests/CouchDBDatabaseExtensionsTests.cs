using Xunit;
using Moq;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using static CouchDB.Client.Tests.AssertHelper;

namespace CouchDB.Client.Tests
{
    public sealed class CouchDBDatabaseExtensionsTests
    {
        private readonly Mock<ICouchDBDatabase> _sut;

        public CouchDBDatabaseExtensionsTests()
        {
            _sut = new Mock<ICouchDBDatabase>(); 
        }

        #region Save JSON doc

        [Fact]
        public async void SaveJsonDocumentAsync_Requires_Json_Document()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentAsync((JObject)null, new DocUpdateParams()));
        }

        [Fact]
        public async void SaveJsonDocumentAsync_Passes_UpdateParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var updateParams = new DocUpdateParams();

            //act.
            await _sut.Object.SaveDocumentAsync(JObject.FromObject(new { }), updateParams);

            //assert.
            _sut.Verify(db => db.SaveDocumentAsync(It.IsAny<string>(), updateParams), Times.Once());
        }

        [Fact]
        public async void SaveJsonDocumentAsync_Sets_ID_And_Revision_After_Save()
        {
            //arrange.
            var saveResponse = new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO
            {
                Id = "some id",
                Rev = "some rev"
            });

            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(saveResponse));

            //act.
            var jsonDoc = JObject.FromObject(new { Name = "value" });
            await _sut.Object.SaveDocumentAsync(jsonDoc);

            //assert.
            Assert.Equal(saveResponse.Id, jsonDoc[CouchDBDatabase.IdPropertyName]);
            Assert.Equal(saveResponse.Revision, jsonDoc[CouchDBDatabase.RevisionPropertyName]);
        }

        #endregion

        #region Save Object doc

        [Fact]
        public async void SaveObjectDocumentAsync_Requires_Object_Document()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentAsync((object)null, new DocUpdateParams()));
        }

        [Fact]
        public async void SaveObjectDocumentAsync_Removes_ID_If_its_Empty()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var doc = new { _id = string.Empty, SomeProperty = "some value" };

            //act.
            await _sut.Object.SaveDocumentAsync(doc);

            //assert.
            var docWithoutId = JObject.FromObject(new { SomeProperty = "some value" });
            _sut.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, docWithoutId)), It.IsAny<DocUpdateParams>()), Times.Once());
        }

        [Fact]
        public async void SaveObjectDocumentAsync_Passes_UpdateParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var updateParams = new DocUpdateParams();

            //act.
            await _sut.Object.SaveDocumentAsync(new { }, updateParams);

            //assert.
            _sut.Verify(db => db.SaveDocumentAsync(It.IsAny<string>(), updateParams), Times.Once());
        }

        [Fact]
        public async void SaveObjectDocumentAsync_Returns_ID_And_Revision_After_Save()
        {
            //arrange.
            var saveResponse = new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO
            {
                Id = "some id",
                Rev = "some rev"
            });

            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(saveResponse));

            //act.
            var objectDoc = new { Name = "value" };
            var result = await _sut.Object.SaveDocumentAsync(objectDoc);

            //assert.
            Assert.Equal(saveResponse.Id, result.Id);
            Assert.Equal(saveResponse.Revision, result.Revision);
        }

        #endregion

        #region Get JSON doc

        [Fact]
        public async void GetDocumentJsonAsync_Requires_DocId()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetDocumentJsonAsync(null, new DocQueryParams()));
        }

        [Fact]
        public async void GetDocumentJsonAsync_Passes_ID_and_QueryParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult("{ }"));

            var docId = "some id";
            var queryParams = new DocQueryParams();

            //act.
            await _sut.Object.GetDocumentJsonAsync(docId, queryParams);

            //assert.
            _sut.Verify(db => db.GetDocumentAsync(docId, queryParams), Times.Once());
        }

        [Fact]
        public async void GetDocumentJsonAsync_Retrieves_Document()
        {
            //arrange.
            var docString = JsonConvert.SerializeObject(new { name = "value", name2 = "value2" });
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(docString));

            //act.
            var jsonDoc = await _sut.Object.GetDocumentJsonAsync("id");

            //assert.
            Assert.NotNull(jsonDoc);
            Assert.True(StringIsJsonObject(docString, jsonDoc));
        }

        #endregion

        #region Get Generic doc

        private sealed class SampleDoc
        {
            public string Name { get; set; }

            public int Name2 { get; set; }
        }

        [Fact]
        public async void GetGenericDoc_Requires_DocId()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetDocumentAsync<SampleDoc>(null, new DocQueryParams()));
        }

        [Fact]
        public async void GetGenericDoc_Passes_ID_and_QueryParams()
        {
            //arrange.
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult("{ }"));

            var docId = "some id 123";
            var queryParams = new DocQueryParams();

            //act.
            await _sut.Object.GetDocumentAsync<SampleDoc>(docId, queryParams);

            //assert.
            _sut.Verify(db => db.GetDocumentAsync(docId, queryParams), Times.Once());
        }

        [Fact]
        public async void GetGenericDoc_Retrieves_Document()
        {
            //arrange.
            var docString = JsonConvert.SerializeObject(new SampleDoc { Name = "value1", Name2 = 12321 });
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(docString));

            //act.
            var result = await _sut.Object.GetDocumentAsync<SampleDoc>("id");

            //assert.
            Assert.NotNull(result);
            var jsonResult = JObject.FromObject(result);
            Assert.True(StringIsJsonObject(docString, jsonResult));
        }

        #endregion

        #region Delete JSON doc

        [Fact]
        public async void DeleteJSONDoc_Requires_Document()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.DeleteDocumentAsync(null));
        }

        [Fact]
        public async void DeleteJSONDoc_Requires_ID()
        {
            //arrange.
            var jsonDoc = JObject.FromObject(new { _id = "", _rev = "not empty" });

            //act / assert.
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.DeleteDocumentAsync(jsonDoc));
        }

        [Fact]
        public async void DeleteJSONDoc_Requires_Revision()
        {
            //arrange.
            var jsonDoc = JObject.FromObject(new { _id = "not empty", _rev = "" });

            //act / assert.
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.DeleteDocumentAsync(jsonDoc));
        }

        [Fact]
        public async void DeleteJSONDoc_Passes_ID_and_Revision_and_Batch()
        {
            //arrange.
            var expectedId = "some id 182";
            var expecttedRev = "some rev 12";
            var expectedBatch = true;
            var jsonDoc = JObject.FromObject(new { _id = expectedId, _rev = expecttedRev });

            _sut.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = "id123", Rev = "rev123" })));

            //act.
            await _sut.Object.DeleteDocumentAsync(jsonDoc, expectedBatch);

            //assert.
            _sut.Verify(db => db.DeleteDocumentAsync(expectedId, expecttedRev, expectedBatch), Times.Once());
        }

        [Fact]
        public async void DeleteJSONDoc_UpdatesJSON_With_New_ID_and_Revision()
        {
            //arrange.
            var expectedId = "id 128";
            var expectedRev = "rev 129";

            _sut.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(
                    new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            var jsonDoc = JObject.FromObject(new { _id = "old id", _rev = "old rev" });

            //act.
            await _sut.Object.DeleteDocumentAsync(jsonDoc);

            //assert.
            Assert.Equal(expectedId, jsonDoc[CouchDBDatabase.IdPropertyName]);
            Assert.Equal(expectedRev, jsonDoc[CouchDBDatabase.RevisionPropertyName]);
        }

        #endregion

        #region Get All Object Docs

        [Fact]
        public async void GetAllObjectDocumentsAsync_Passes_QueryParams_AsReceived()
        {
            //arrange.
            var queryParams = new ListQueryParams();

            _sut.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.Object.GetAllObjectDocumentsAsync<SampleDoc>(queryParams);

            //assert.
            _sut.Verify(db => db.GetAllDocumentsAsync(queryParams), Times.Once());
        }

        [Fact]
        public async void GetAllObjectDocumentsAsync_Returns_Deserialized_Documents_ByDefault()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<string>("id", "key", new DocListResponseRowValue("rev"), JsonConvert.SerializeObject(doc), null)))));

            //act.
            var docs = await _sut.Object.GetAllObjectDocumentsAsync<SampleDoc>(null, null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].name, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].name2, docs.Rows[0].Document.Name2);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].name, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].name2, docs.Rows[0].Document.Name2);
        }

        [Fact]
        public async void GetAllObjectDocumentsAsync_Deserializes_Using_Deserializer_If_Provided()
        {
            //arrange.
            var jsonDocs = new[] {
                new { what = "some value", that = 111 },
                new { what = "some value", that = 222 }
            };
            _sut.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<string>("id", "key", new DocListResponseRowValue("rev"), JsonConvert.SerializeObject(doc), null)))));

            Func<string, SampleDoc> deserializer = doc => 
            {
                var json = JObject.Parse(doc);
                return new SampleDoc { Name = json["what"].ToString(), Name2 = json["that"].Value<int>() };
            };

            //act.
            var docs = await _sut.Object.GetAllObjectDocumentsAsync(null, deserializer);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].what, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].that, docs.Rows[0].Document.Name2);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].what, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].that, docs.Rows[0].Document.Name2);
        }

        #endregion

        #region Get All String Docs

        [Fact]
        public async void GetAllJsonDocumentsAsync_Passes_QueryParams_AsReceived()
        {
            //arrange
            var queryParams = new ListQueryParams();

            _sut.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.Object.GetAllJsonDocumentsAsync(queryParams);

            //assert.
            _sut.Verify(db => db.GetAllDocumentsAsync(queryParams), Times.Once());
        }

        [Fact]
        public async void GetAllJsonDocumentsAsync_Returns_JsonRepresentation_OfDocuments()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<string>("id", "key", new DocListResponseRowValue("rev"), JsonConvert.SerializeObject(doc), null)))));

            //act.
            var docs = await _sut.Object.GetAllJsonDocumentsAsync(null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[0]), docs.Rows[0].Document));
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[1]), docs.Rows[1].Document));
        }

        #endregion

        #region Get Object Docs

        [Fact]
        public async void GetObjectDocumentsAsync_Requires_NotEmpty_IdList()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetObjectDocumentsAsync<SampleDoc>(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.GetObjectDocumentsAsync<SampleDoc>(new string[] { }));
        }

        [Fact]
        public async void GetObjectDocumentsAsync_Passes_Params_AsReceived()
        {
            //arrange.
            var docIdList = new string[] { "id-1" };
            var queryParams = new ListQueryParams();

            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            await _sut.Object.GetObjectDocumentsAsync<SampleDoc>(docIdList, queryParams);

            //assert.
            _sut.Verify(db => db.GetJsonDocumentsAsync(docIdList, queryParams), Times.Once());
        }

        [Fact]
        public async void GetObjectDocumentsAsync_Returns_Deserialized_Documents_ByDefault()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

            //act.
            var docs = await _sut.Object.GetObjectDocumentsAsync<SampleDoc>(new string[] { "id-1" }, null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].name, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].name2, docs.Rows[0].Document.Name2);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].name, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].name2, docs.Rows[0].Document.Name2);
        }

        [Fact]
        public async void GetObjectDocumentsAsync_Deserializes_Using_Deserializer_If_Provided()
        {
            //arrange.
            var jsonDocs = new[] {
                new { what = "some value", that = 111 },
                new { what = "some value", that = 222 }
            };
            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

            Func<JObject, SampleDoc> deserializer = doc => new SampleDoc { Name = doc["what"].ToString(), Name2 = doc["that"].Value<int>() };

            //act.
            var docs = await _sut.Object.GetObjectDocumentsAsync<SampleDoc>(new string[] { "id-1" }, null, deserializer);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].what, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].that, docs.Rows[0].Document.Name2);

            Assert.NotNull(docs.Rows[0]);
            Assert.Equal(jsonDocs[0].what, docs.Rows[0].Document.Name);
            Assert.Equal(jsonDocs[0].that, docs.Rows[0].Document.Name2);
        }

        [Fact]
        public async void GetObjectDocumentsAsync_Returns_Null_Document_For_Error_Row()
        {
            //arrange.
            var expectedError = new List<DocListResponseRow<JObject>>() { new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("revision"), null, new ServerResponseError("some_error")) };

            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, expectedError)));

            //act.
            var docs = await _sut.Object.GetObjectDocumentsAsync<SampleDoc>(new string[] { "id-1" }, null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(1, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Null(docs.Rows[0].Document);
            Assert.NotNull(docs.Rows[0].Error);
            Assert.Same(expectedError[0].Error, docs.Rows[0].Error);
        }

        #endregion

        #region Get String Docs

        [Fact]
        public async void GetStringDocumentsAsync_Requires_NotEmpty_IdList()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetStringDocumentsAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.GetStringDocumentsAsync(new string[] { }));
        }

        [Fact]
        public async void GetStringDocumentsAsync_Passes_Params_AsReceived()
        {
            //arrange
            var docIdList = new string[] { "id1" };
            var queryParams = new ListQueryParams();

            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            await _sut.Object.GetStringDocumentsAsync(docIdList, queryParams);

            //assert.
            _sut.Verify(db => db.GetJsonDocumentsAsync(docIdList, queryParams), Times.Once());
        }

        [Fact]
        public async void GetStringDocumentsAsync_Returns_StringRepresentation_OfObjects()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

            //act.
            var docs = await _sut.Object.GetStringDocumentsAsync(new string[] { "id-1" }, null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[0]), JObject.Parse(docs.Rows[0].Document)));
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[1]), JObject.Parse(docs.Rows[1].Document)));
        }

        [Fact]
        public async void GetStringDocumentsAsync_Returns_Null_Document_For_Error_Row()
        {
            //arrange.
            var expectedError = new List<DocListResponseRow<JObject>>() { new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("revision"), null, new ServerResponseError("some_error")) };

            _sut.Setup(db => db.GetJsonDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, expectedError)));

            //act.
            var docs = await _sut.Object.GetStringDocumentsAsync(new string[] { "id-1" }, null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(1, docs.Rows.Count);

            Assert.NotNull(docs.Rows[0]);
            Assert.Null(docs.Rows[0].Document);
            Assert.NotNull(docs.Rows[0].Error);
            Assert.Same(expectedError[0].Error, docs.Rows[0].Error);
        }

        #endregion

        #region Save JSON Docs

        [Fact]
        public async void SaveDocumentsAsync_Requires_Documents()
        {
            //act / assert.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentsAsync((JObject[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentsAsync(new JObject[] { }));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveDocumentsAsync_Passes_Documents_AsStrings_And_NewEditsFlag(bool newEdits)
        {
            //arrange.
            var docs = new JObject[]
            {
                JObject.FromObject(new { id = 123, name = "name 123" }),
                JObject.FromObject(new { id2 = 1232, name2 = "name 123 2" })
            };

            //act.
            await _sut.Object.SaveDocumentsAsync(docs, newEdits);

            //assert.
            Predicate<string[]> areDocsFromJson = stringDocs => stringDocs.All(s => docs.Any(d => StringIsJsonObject(s, d)));

            _sut.Verify(db => db.SaveDocumentsAsync(It.Is<string[]>(strDocs => areDocsFromJson(strDocs)), newEdits), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveDocumentsAsync_Returns_Same_Result_As_DB_Object(bool newEdits)
        {
            //arrange.
            var expectedResponse = new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO());

            _sut.Setup(db => db.SaveDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(expectedResponse));

            //act.
            var result = await _sut.Object.SaveDocumentsAsync(new JObject[] { JObject.FromObject(new { id = 123 }) }, newEdits);

            //assert.
            Assert.Same(expectedResponse, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveDocumentsAsync_Updates_ID_And_Rev_Of_Documents_With_Success(bool newEdits)
        {
            //arrange.
            var objectDocs = new [] 
            {
                new { _id = "1", _rev = "1", name = "name 1" },
                new { _id = "2", _rev = "2", name = "name 2" }
            };

            var docs = new JObject[]
            {
                JObject.FromObject(objectDocs[0]),
                JObject.FromObject(objectDocs[1])
            };

            var expectedResponse = new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO
            {
                new CouchDBDatabase.SaveDocResponseDTO
                {
                    Id = "id 1", Rev = "rev 1", Error = "some error", Reason = "some reason"
                },
                new CouchDBDatabase.SaveDocResponseDTO
                {
                    Id = "id 2", Rev = "rev 2"
                }
            });

            _sut.Setup(db => db.SaveDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(expectedResponse));

            //act.
            await _sut.Object.SaveDocumentsAsync(docs, newEdits);

            //assert.

            //first doc did not change due to error.
            Assert.Equal(objectDocs[0]._id, docs[0]["_id"]);
            Assert.Equal(objectDocs[0]._rev, docs[0]["_rev"]);

            //second doc changed (no error).
            Assert.Equal(expectedResponse.DocumentResponses[1].Id, docs[1]["_id"].ToString());
            Assert.Equal(expectedResponse.DocumentResponses[1].Revision, docs[1]["_rev"].ToString());
        }

        #endregion

        #region Save Object Docs

        [Fact]
        public async void SaveObjDocumentsAsync_Requires_Documents()
        {
            //act / assert.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentsAsync((object[])null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentsAsync(new object[] { }));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveObjDocumentsAsync_Passes_Documents_AsStrings_And_NewEditsFlag(bool newEdits)
        {
            //arrange.
            var docs = new object[]
            {
                new { id = 123, name = "name 123" },
                new { id2 = 1232, name2 = "name 123 2" }
            };

            //act.
            await _sut.Object.SaveDocumentsAsync(docs, newEdits);

            //assert.
            Predicate<string[]> areDocsFromObject = stringDocs => stringDocs.All(s => docs.Any(d => StringIsJsonObject(s, JObject.FromObject(d))));

            _sut.Verify(db => db.SaveDocumentsAsync(It.Is<string[]>(strDocs => areDocsFromObject(strDocs)), newEdits), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveObjDocumentsAsync_Returns_Same_Result_As_DB_Object(bool newEdits)
        {
            //arrange.
            var expectedResponse = new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO());

            _sut.Setup(db => db.SaveDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(expectedResponse));

            //act.
            var result = await _sut.Object.SaveDocumentsAsync(new object[] { new { id = 123 } }, newEdits);

            //assert.
            Assert.Same(expectedResponse, result);
        }

        #endregion
    }
}
