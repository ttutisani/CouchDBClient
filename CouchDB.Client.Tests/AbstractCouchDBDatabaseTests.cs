using Xunit;
using Moq;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CouchDB.Client.Tests
{
    public sealed class AbstractCouchDBDatabaseTests
    {
        private readonly Mock<AbstractCuchDBDatabase> _sut;

        public AbstractCouchDBDatabaseTests()
        {
            _sut = new Mock<AbstractCuchDBDatabase>(); 
        }

        #region Save JSON doc

        [Fact]
        public void SaveJsonDocumentAsync_Requires_Json_Document()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentAsync((JObject)null, new DocUpdateParams())).GetAwaiter().GetResult();
        }

        [Fact]
        public void SaveJsonDocumentAsync_Passes_UpdateParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var updateParams = new DocUpdateParams();

            //act.
            _sut.Object.SaveDocumentAsync(JObject.FromObject(new { }), updateParams).GetAwaiter().GetResult();

            //assert.
            _sut.Verify(db => db.SaveDocumentAsync(It.IsAny<string>(), updateParams), Times.Once());
        }

        [Fact]
        public void SaveJsonDocumentAsync_Sets_ID_And_Revision_After_Save()
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
            _sut.Object.SaveDocumentAsync(jsonDoc).GetAwaiter().GetResult();

            //assert.
            Assert.Equal(saveResponse.Id, jsonDoc[AbstractCuchDBDatabase.IdPropertyName]);
            Assert.Equal(saveResponse.Revision, jsonDoc[AbstractCuchDBDatabase.RevisionPropertyName]);
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
        public void SaveObjectDocumentAsync_Requires_Object_Document()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentAsync((object)null, new DocUpdateParams())).GetAwaiter().GetResult();
        }

        [Fact]
        public void SaveObjectDocumentAsync_Removes_ID_If_its_Empty()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var doc = new { _id = string.Empty, SomeProperty = "some value" };

            //act.
            _sut.Object.SaveDocumentAsync(doc).GetAwaiter().GetResult();

            //assert.
            var docWithoutId = JObject.FromObject(new { SomeProperty = "some value" });
            _sut.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, docWithoutId)), It.IsAny<DocUpdateParams>()), Times.Once());
        }

        [Fact]
        public void SaveObjectDocumentAsync_Passes_UpdateParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var updateParams = new DocUpdateParams();

            //act.
            _sut.Object.SaveDocumentAsync(new { }, updateParams).GetAwaiter().GetResult();

            //assert.
            _sut.Verify(db => db.SaveDocumentAsync(It.IsAny<string>(), updateParams), Times.Once());
        }

        [Fact]
        public void SaveObjectDocumentAsync_Returns_ID_And_Revision_After_Save()
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
            var result = _sut.Object.SaveDocumentAsync(objectDoc).GetAwaiter().GetResult();

            //assert.
            Assert.Equal(saveResponse.Id, result.Id);
            Assert.Equal(saveResponse.Revision, result.Revision);
        }

        #endregion

        #region Get JSON doc

        [Fact]
        public void GetDocumentJsonAsync_Requires_DocId()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetDocumentJsonAsync(null, new DocQueryParams())).GetAwaiter().GetResult();
        }

        [Fact]
        public void GetDocumentJsonAsync_Passes_ID_and_QueryParams_AsReceived()
        {
            //arrange.
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult("{ }"));

            var docId = "some id";
            var queryParams = new DocQueryParams();

            //act.
            _sut.Object.GetDocumentJsonAsync(docId, queryParams).GetAwaiter().GetResult();

            //assert.
            _sut.Verify(db => db.GetDocumentAsync(docId, queryParams), Times.Once());
        }

        [Fact]
        public void GetDocumentJsonAsync_Retrieves_Document()
        {
            //arrange.
            var docString = JsonConvert.SerializeObject(new { name = "value", name2 = "value2" });
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(docString));

            //act.
            var jsonDoc = _sut.Object.GetDocumentJsonAsync("id").GetAwaiter().GetResult();

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
        public void GetGenericDoc_Requires_DocId()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.GetDocumentAsync<SampleDoc>(null, new DocQueryParams()))
                .GetAwaiter().GetResult();
        }

        [Fact]
        public void GetGenericDoc_Passes_ID_and_QueryParams()
        {
            //arrange.
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult("{ }"));

            var docId = "some id 123";
            var queryParams = new DocQueryParams();

            //act.
            _sut.Object.GetDocumentAsync<SampleDoc>(docId, queryParams).GetAwaiter().GetResult();

            //assert.
            _sut.Verify(db => db.GetDocumentAsync(docId, queryParams), Times.Once());
        }

        [Fact]
        public void GetGenericDoc_Retrieves_Document()
        {
            //arrange.
            var docString = JsonConvert.SerializeObject(new SampleDoc { Name = "value1", Name2 = 12321 });
            _sut.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(docString));

            //act.
            var result = _sut.Object.GetDocumentAsync<SampleDoc>("id").GetAwaiter().GetResult();

            //assert.
            Assert.NotNull(result);
            var jsonResult = JObject.FromObject(result);
            Assert.True(StringIsJsonObject(docString, jsonResult));
        }

        #endregion

        #region Delete JSON doc

        [Fact]
        public void DeleteJSONDoc_Requires_Document()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.DeleteDocumentAsync(null))
                .GetAwaiter().GetResult();
        }

        [Fact]
        public void DeleteJSONDoc_Requires_ID()
        {
            //arrange.
            var jsonDoc = JObject.FromObject(new { _id = "", _rev = "not empty" });

            //act / assert.
            Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.DeleteDocumentAsync(jsonDoc))
                .GetAwaiter().GetResult();
        }

        [Fact]
        public void DeleteJSONDoc_Requires_Revision()
        {
            //arrange.
            var jsonDoc = JObject.FromObject(new { _id = "not empty", _rev = "" });

            //act / assert.
            Assert.ThrowsAsync<ArgumentException>(() => _sut.Object.DeleteDocumentAsync(jsonDoc))
                .GetAwaiter().GetResult();
        }

        [Fact]
        public void DeleteJSONDoc_Passes_ID_and_Revision_and_Batch()
        {
            //arrange.
            var expectedId = "some id 182";
            var expecttedRev = "some rev 12";
            var expectedBatch = true;
            var jsonDoc = JObject.FromObject(new { _id = expectedId, _rev = expecttedRev });

            _sut.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = "id123", Rev = "rev123" })));

            //act.
            _sut.Object.DeleteDocumentAsync(jsonDoc, expectedBatch)
                .GetAwaiter().GetResult();

            //assert.
            _sut.Verify(db => db.DeleteDocumentAsync(expectedId, expecttedRev, expectedBatch), Times.Once());
        }

        [Fact]
        public void DeleteJSONDoc_UpdatesJSON_With_New_ID_and_Revision()
        {
            //arrange.
            var expectedId = "id 128";
            var expectedRev = "rev 129";

            _sut.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(
                    new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            var jsonDoc = JObject.FromObject(new { _id = "old id", _rev = "old rev" });

            //act.
            _sut.Object.DeleteDocumentAsync(jsonDoc)
                .GetAwaiter().GetResult();

            //assert.
            Assert.Equal(expectedId, jsonDoc[AbstractCuchDBDatabase.IdPropertyName]);
            Assert.Equal(expectedRev, jsonDoc[AbstractCuchDBDatabase.RevisionPropertyName]);
        }

        #endregion
    }
}
