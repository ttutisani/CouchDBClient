using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class EntityStoreTests
    {
        private readonly Mock<ICouchDBDatabase> _db;
        private readonly EntityStore _sut;

        public EntityStoreTests()
        {
            _db = new Mock<ICouchDBDatabase>();
            _sut = new EntityStore(_db.Object);
        }

        [Fact]
        public void Ctor_Requires_DB()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityStore(null));
        }

        private sealed class SampleEntity : IEntity
        {
            public string _id
            {
                get;
                set;
            }

            public string _rev
            {
                get;
                set;
            }

            public string Name { get; set; }
            public int Name2 { get; set; }
        }

        [Fact]
        public void SaveEntityAsync_Requires_Entity()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveEntityAsync(null));
        }

        [Fact]
        public void SaveEntityAsync_Removes_Rev_If_Empty()
        {
            //arrange.
            var entity = new SampleEntity {
                _id = "id123",
                _rev = null,
                Name = "some name",
                Name2 = 123
            };

            var entityWithoutRev = new { entity._id, entity.Name, entity.Name2 };

            _db.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            //act.
            _sut.SaveEntityAsync(entity).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, JObject.FromObject(entityWithoutRev))), It.IsAny<DocUpdateParams>()), Times.Once());
        }

        [Fact]
        public void SaveEntityAsync_Passes_Entity_And_Params()
        {
            //arrange.
            var entity = new SampleEntity { _id = "id123", _rev = "rev123", Name = "somename", Name2 = 213 };
            var entityJson = JsonConvert.SerializeObject(entity);
            var updateParams = new DocUpdateParams();

            _db.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            //act.
            _sut.SaveEntityAsync(entity, updateParams).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, JObject.Parse(entityJson))), updateParams), Times.Once());
        }

        [Fact]
        public void SaveEntityAsync_Sets_ID_And_Rev_After_Save()
        {
            //arrange.
            var entity = new SampleEntity();
            var expectedId = "idnew";
            var expectedRev = "revnew";

            _db.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            //act.
            _sut.SaveEntityAsync(entity, null).GetAwaiter().GetResult();

            //assert.
            Assert.Equal(expectedId, entity._id);
            Assert.Equal(expectedRev, entity._rev);
        }

        [Fact]
        public void GetEntityAsync_Requires_ID()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetEntityAsync<SampleEntity>(null, new DocQueryParams()));
        }

        [Fact]
        public void GetEntityAsync_Passes_ID_And_Query_Params()
        {
            //arrange.
            var id = "entity id";
            var queryParams = new DocQueryParams();

            //act.
            _sut.GetEntityAsync<SampleEntity>(id, queryParams).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.GetDocumentAsync(id, queryParams), Times.Once());
        }

        [Fact]
        public void GetEntityAsync_Returns_Document_As_Entity()
        {
            //arrange.
            var expectedJson = JsonConvert.SerializeObject(new SampleEntity { _id = "what", _rev = "ever", Name = "name", Name2 = 123 });
            _db.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(expectedJson));

            //act.
            var entity = _sut.GetEntityAsync<SampleEntity>("someid").GetAwaiter().GetResult();

            //assert.
            Assert.True(StringIsJsonObject(expectedJson, JObject.FromObject(entity)));
        }

        [Fact]
        public void GetAllEntitiesAsync_Makes_QueryParams_NotNull_And_Extracts_Docs()
        {
            //arrange.
            _db.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse2<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            _sut.GetAllEntitiesAsync<SampleEntity>(null).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.GetAllJsonDocumentsAsync(It.Is<ListQueryParams>(p => p != null && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public void GetAllEntitiesAsync_Passes_QueryParams_And_Extracts_Docs()
        {
            //arrange.
            var queryParams = new ListQueryParams();

            _db.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse2<JObject>(0, 100, 1, Enumerable.Empty<DocListResponseRow<JObject>>())));

            //act.
            _sut.GetAllEntitiesAsync<SampleEntity>(queryParams).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.GetAllJsonDocumentsAsync(It.Is<ListQueryParams>(p => p == queryParams && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public void GetAllEntitiesAsync_Returns_Docs_As_Entities()
        {
            //arrange.
            var expectedDocs = new[] 
            {
                new SampleEntity { _id = "id1", _rev = "rev1", Name = "name1", Name2 = 1 },
                new SampleEntity { _id = "id2", _rev = "rev2", Name = "name2", Name2 = 2 }
            };

            _db.Setup(db => db.GetAllJsonDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse2<JObject>(0, 100, 1, new List<DocListResponseRow<JObject>>
                {
                    new DocListResponseRow<JObject>("id1", "id1", new DocListResponseRowValue("rev1"), JObject.FromObject(expectedDocs[0]), null),
                    new DocListResponseRow<JObject>("id2", "id2", new DocListResponseRowValue("rev2"), JObject.FromObject(expectedDocs[1]), null)
                })));

            //act.
            var entities = _sut.GetAllEntitiesAsync<SampleEntity>(null).GetAwaiter().GetResult();

            //assert.
            Assert.Equal(2, entities.Rows.Count);

            Assert.NotNull(entities.Rows[0]);
            Assert.NotNull(entities.Rows[0].Document);
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(expectedDocs[0]), JObject.FromObject(entities.Rows[0].Document)));

            Assert.NotNull(entities.Rows[1]);
            Assert.NotNull(entities.Rows[1].Document);
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(expectedDocs[1]), JObject.FromObject(entities.Rows[1].Document)));
        }

        [Fact]
        public void DeleteEntityAsync_Requires_Entity()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteEntityAsync(null));
        }

        [Fact]
        public void DeleteEntityAsync_Passes_Entity_And_Batch_Flag()
        {
            //arrange.
            var id = "some id 123";
            var rev = "some rev 123";
            var entity = new SampleEntity { _id = id, _rev = rev };
            var batch = true;

            _db.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            //act.
            _sut.DeleteEntityAsync(entity, batch).GetAwaiter().GetResult();

            //assert.
            _db.Verify(db => db.DeleteDocumentAsync(id, rev, batch), Times.Once());
        }

        [Fact]
        public void DeleteEntityAsync_Updates_ID_And_Rev_After_Deletion()
        {
            //arrange.
            var expectedId = "id 1212";
            var expectedRev = "rev 18u2";

            _db.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            var entity = new SampleEntity();

            //act.
            _sut.DeleteEntityAsync(entity).GetAwaiter().GetResult();

            //assert.
            Assert.Equal(expectedId, entity._id);
            Assert.Equal(expectedRev, entity._rev);
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
