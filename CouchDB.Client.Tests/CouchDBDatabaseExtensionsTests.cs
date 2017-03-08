﻿using Xunit;
using Moq;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

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

        private static bool StringIsJsonObject(string value, JObject json)
        {
            if (value == null && json == null)
                return true;

            if (value == null || json == null)
                return false;

            var valueJson = JObject.Parse(value);

            return JToken.DeepEquals(json, valueJson);
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

            _sut.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            await _sut.Object.GetAllObjectDocumentsAsync<SampleDoc>(queryParams);

            //assert.
            _sut.Verify(db => db.GetAllJsonDocumentsAsync(queryParams), Times.Once());
        }

        [Fact]
        public async void GetAllObjectDocumentsAsync_Returns_Deserialized_Documents_ByDefault()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

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
            _sut.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

            Func<JObject, SampleDoc> deserializer = doc => new SampleDoc { Name = doc["what"].ToString(), Name2 = doc["that"].Value<int>() };

            //act.
            var docs = await _sut.Object.GetAllObjectDocumentsAsync<SampleDoc>(null, deserializer);

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
        public async void GetAllStringDocumentsAsync_Passes_QueryParams_AsReceived()
        {
            //arrange
            var queryParams = new ListQueryParams();

            _sut.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            await _sut.Object.GetAllStringDocumentsAsync(queryParams);

            //assert.
            _sut.Verify(db => db.GetAllJsonDocumentsAsync(queryParams), Times.Once());
        }

        [Fact]
        public async void GetAllStringDocumentsAsync_Returns_StringRepresentation_OfObjects()
        {
            //arrange.
            var jsonDocs = new[] {
                new { name = "some value", name2 = 111 },
                new { name = "some value", name2 = 222 }
            };
            _sut.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<JObject>(0, 100, 1, jsonDocs.Select(doc => new DocListResponseRow<JObject>("id", "key", new DocListResponseRowValue("rev"), JObject.FromObject(doc), null)))));

            //act.
            var docs = await _sut.Object.GetAllStringDocumentsAsync(null);

            //assert.
            Assert.NotNull(docs);
            Assert.Equal(2, docs.Rows.Count);

            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[0]), JObject.Parse(docs.Rows[0].Document)));
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(jsonDocs[1]), JObject.Parse(docs.Rows[1].Document)));
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
    }
}
