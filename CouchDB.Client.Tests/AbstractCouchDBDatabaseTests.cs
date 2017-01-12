using Xunit;
using Moq;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CouchDB.Client.Tests
{
    public sealed class AbstractCouchDBDatabaseTests
    {
        private readonly Mock<AbstractCuchDBDatabase> _sut;

        public AbstractCouchDBDatabaseTests()
        {
            _sut = new Mock<AbstractCuchDBDatabase>(); 
        }

        [Fact]
        public void SaveDocumentAsync_Requires_Json_Document()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Object.SaveDocumentAsync((JObject)null));
        }

        [Fact]
        public void SaveDocumentAsync_Removes_ID_If_its_Empty()
        {
            //arrange.
            _sut.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            var doc = JObject.FromObject(new { _id = string.Empty, SomeProperty = "some value" });

            //act.
            _sut.Object.SaveDocumentAsync(doc).GetAwaiter().GetResult();

            //assert.
            var docWithoutId = JObject.FromObject(new { SomeProperty = "some value" });
            _sut.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, docWithoutId)), It.IsAny<DocUpdateParams>()), Times.Once());
        }

        [Fact]
        public void SaveDocumentAsync_Passes_UpdateParams_AsReceived()
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
        public void SaveDocumentAsync_Sets_ID_And_Revision_After_Save()
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
    }
}
