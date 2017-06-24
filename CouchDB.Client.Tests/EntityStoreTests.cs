using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static CouchDB.Client.Tests.AssertHelper;

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
        public async void SaveEntityAsync_Requires_Entity()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveEntityAsync(null));
        }

        [Fact]
        public async void SaveEntityAsync_Removes_Rev_If_Empty()
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
            await _sut.SaveEntityAsync(entity);

            //assert.
            _db.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, JObject.FromObject(entityWithoutRev))), It.IsAny<DocUpdateParams>()), Times.Once());
        }

        [Fact]
        public async void SaveEntityAsync_Passes_Entity_And_Params()
        {
            //arrange.
            var entity = new SampleEntity { _id = "id123", _rev = "rev123", Name = "somename", Name2 = 213 };
            var entityJson = JsonConvert.SerializeObject(entity);
            var updateParams = new DocUpdateParams();

            _db.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            //act.
            await _sut.SaveEntityAsync(entity, updateParams);

            //assert.
            _db.Verify(db => db.SaveDocumentAsync(It.Is<string>(str => StringIsJsonObject(str, JObject.Parse(entityJson))), updateParams), Times.Once());
        }

        [Fact]
        public async void SaveEntityAsync_Sets_ID_And_Rev_After_Save()
        {
            //arrange.
            var entity = new SampleEntity();
            var expectedId = "idnew";
            var expectedRev = "revnew";

            _db.Setup(db => db.SaveDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            //act.
            await _sut.SaveEntityAsync(entity, null);

            //assert.
            Assert.Equal(expectedId, entity._id);
            Assert.Equal(expectedRev, entity._rev);
        }

        [Fact]
        public async void GetEntityAsync_Requires_ID()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetEntityAsync<SampleEntity>(null, new DocQueryParams()));
        }

        [Fact]
        public async void GetEntityAsync_Passes_ID_And_Query_Params()
        {
            //arrange.
            var id = "entity id";
            var queryParams = new DocQueryParams();

            //act.
            await _sut.GetEntityAsync<SampleEntity>(id, queryParams);

            //assert.
            _db.Verify(db => db.GetDocumentAsync(id, queryParams), Times.Once());
        }

        [Fact]
        public async void GetEntityAsync_Returns_Document_As_Entity()
        {
            //arrange.
            var expectedJson = JsonConvert.SerializeObject(new SampleEntity { _id = "what", _rev = "ever", Name = "name", Name2 = 123 });
            _db.Setup(db => db.GetDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(expectedJson));

            //act.
            var entity = await _sut.GetEntityAsync<SampleEntity>("someid");

            //assert.
            Assert.True(StringIsJsonObject(expectedJson, JObject.FromObject(entity)));
        }

        [Fact]
        public async void GetAllEntitiesAsync_Makes_QueryParams_NotNull_And_Extracts_Docs()
        {
            //arrange.
            _db.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.GetAllEntitiesAsync<SampleEntity>(null);

            //assert.
            _db.Verify(db => db.GetAllDocumentsAsync(It.Is<ListQueryParams>(p => p != null && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public async void GetAllEntitiesAsync_Passes_QueryParams_And_Extracts_Docs()
        {
            //arrange.
            var queryParams = new ListQueryParams();

            _db.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.GetAllEntitiesAsync<SampleEntity>(queryParams);

            //assert.
            _db.Verify(db => db.GetAllDocumentsAsync(It.Is<ListQueryParams>(p => p == queryParams && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public async void GetAllEntitiesAsync_Returns_Docs_As_Entities()
        {
            //arrange.
            var expectedDocs = new[] 
            {
                new SampleEntity { _id = "id1", _rev = "rev1", Name = "name1", Name2 = 1 },
                new SampleEntity { _id = "id2", _rev = "rev2", Name = "name2", Name2 = 2 }
            };

            _db.Setup(db => db.GetAllDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, new List<DocListResponseRow<string>>
                {
                    new DocListResponseRow<string>("id1", "id1", new DocListResponseRowValue("rev1"), JsonConvert.SerializeObject(expectedDocs[0]), null),
                    new DocListResponseRow<string>("id2", "id2", new DocListResponseRowValue("rev2"), JsonConvert.SerializeObject(expectedDocs[1]), null)
                })));

            //act.
            var entities = await _sut.GetAllEntitiesAsync<SampleEntity>(null);

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
        public async void DeleteEntityAsync_Requires_Entity()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.DeleteEntityAsync(null));
        }

        [Fact]
        public async void DeleteEntityAsync_Passes_Entity_And_Batch_Flag()
        {
            //arrange.
            var id = "some id 123";
            var rev = "some rev 123";
            var entity = new SampleEntity { _id = id, _rev = rev };
            var batch = true;

            _db.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));

            //act.
            await _sut.DeleteEntityAsync(entity, batch);

            //assert.
            _db.Verify(db => db.DeleteDocumentAsync(id, rev, batch), Times.Once());
        }

        [Fact]
        public async void DeleteEntityAsync_Updates_ID_And_Rev_After_Deletion()
        {
            //arrange.
            var expectedId = "id 1212";
            var expectedRev = "rev 18u2";

            _db.Setup(db => db.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Id = expectedId, Rev = expectedRev })));

            var entity = new SampleEntity();

            //act.
            await _sut.DeleteEntityAsync(entity);

            //assert.
            Assert.Equal(expectedId, entity._id);
            Assert.Equal(expectedRev, entity._rev);
        }

        [Fact]
        public async void GetEntitiesAsync_Requires_Not_Empty_Entity_Id_List()
        {
            //assert.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetEntitiesAsync<SampleEntity>(null));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetEntitiesAsync<SampleEntity>(new string[] { }));
        }

        [Fact]
        public async void GetEntitiesAsync_Makes_QueryParams_NotNull_And_Extracts_Docs()
        {
            //arrange.
            _db.Setup(db => db.GetDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.GetEntitiesAsync<SampleEntity>(new string[] { "id-1" });

            //assert.
            _db.Verify(db => db.GetDocumentsAsync(It.IsAny<string[]>(), It.Is<ListQueryParams>(p => p != null && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public async void GetEntitiesAsync_Passes_IDList_And_QueryParams_And_Extracts_Docs()
        {
            //arrange.
            var idList = new string[] { "id-1" };
            var queryParams = new ListQueryParams();

            _db.Setup(db => db.GetDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, Enumerable.Empty<DocListResponseRow<string>>())));

            //act.
            await _sut.GetEntitiesAsync<SampleEntity>(idList, queryParams);

            //assert.
            _db.Verify(db => db.GetDocumentsAsync(idList, It.Is<ListQueryParams>(p => p == queryParams && p.Include_Docs.GetValueOrDefault())), Times.Once());
        }

        [Fact]
        public async void GetEntitiesAsync_Returns_Docs_As_Entities()
        {
            //arrange.
            var expectedDocs = new[]
            {
                new SampleEntity { _id = "id1", _rev = "rev1", Name = "name1", Name2 = 1 },
                new SampleEntity { _id = "id2", _rev = "rev2", Name = "name2", Name2 = 2 }
            };

            _db.Setup(db => db.GetDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 100, 1, new List<DocListResponseRow<string>>
                {
                    new DocListResponseRow<string>("id1", "id1", new DocListResponseRowValue("rev1"), JsonConvert.SerializeObject(expectedDocs[0]), null),
                    new DocListResponseRow<string>("id2", "id2", new DocListResponseRowValue("rev2"), JsonConvert.SerializeObject(expectedDocs[1]), null)
                })));

            //act.
            var entities = await _sut.GetEntitiesAsync<SampleEntity>(new string[] { "id-1" }, null);

            //assert.
            Assert.Equal(2, entities.Rows.Count);

            Assert.NotNull(entities.Rows[0]);
            Assert.NotNull(entities.Rows[0].Document);
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(expectedDocs[0]), JObject.FromObject(entities.Rows[0].Document)));

            Assert.NotNull(entities.Rows[1]);
            Assert.NotNull(entities.Rows[1].Document);
            Assert.True(StringIsJsonObject(JsonConvert.SerializeObject(expectedDocs[1]), JObject.FromObject(entities.Rows[1].Document)));
        }

        #region Save Object Entities

        [Fact]
        public async void SaveEntitiesAsync_Requires_Entities()
        {
            //act / assert.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveEntitiesAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveEntitiesAsync(new IEntity[] { }));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveEntitiesAsync_Passes_Documents_AsStrings_And_NewEditsFlag(bool newEdits)
        {
            //arrange.
            var docs = new []
            {
                new SampleEntity { _id = "123", _rev = "111", Name = "name 123" },
                new SampleEntity { _id = "1232", _rev = "222", Name2 = 321 }
            };

            //act.
            await _sut.SaveEntitiesAsync(docs, newEdits);

            //assert.
            Predicate<string[]> areDocsFromObject = stringDocs => stringDocs.All(s => docs.Any(d => StringIsJsonObject(s, JObject.FromObject(d))));

            _db.Verify(db => db.SaveDocumentsAsync(It.Is<string[]>(strDocs => areDocsFromObject(strDocs)), newEdits), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveEntitiesAsync_Returns_Same_Result_As_DB_Object(bool newEdits)
        {
            //arrange.
            var expectedResponse = new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO());

            _db.Setup(db => db.SaveDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(expectedResponse));

            //act.
            var result = await _sut.SaveEntitiesAsync(new [] { new SampleEntity { _id = "123" } }, newEdits);

            //assert.
            Assert.Same(expectedResponse, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void SaveEntitiesAsync_Updates_ID_And_Rev_Of_Entities_With_Success(bool newEdits)
        {
            //arrange.
            var entities = new[]
            {
                new SampleEntity { _id = "1", _rev = "1", Name = "name 1" },
                new SampleEntity { _id = "2", _rev = "2", Name = "name 2" }
            };

            var entityClones = entities.Select(e => JObject.FromObject(e)).Select(j => j.ToObject<SampleEntity>()).ToArray();

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

            _db.Setup(db => db.SaveDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(expectedResponse));

            //act.
            await _sut.SaveEntitiesAsync(entities, newEdits);

            //assert.

            //first entity did not change due to error.
            Assert.Equal(entityClones[0]._id, entities[0]._id);
            Assert.Equal(entityClones[0]._rev, entities[0]._rev);

            //second entity changed (no error).
            Assert.Equal(expectedResponse.DocumentResponses[1].Id, entities[1]._id);
            Assert.Equal(expectedResponse.DocumentResponses[1].Revision, entities[1]._rev);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("", true)]
        [InlineData("", false)]
        public async void SaveEntitiesAsync_Does_Not_Pass_Empty_ID_To_Allow_Creation(string id, bool newEdits)
        {
            //arrange.
            var name = "some name";

            //act.
            await _sut.SaveEntitiesAsync(new[] { new SampleEntity { _id = id, Name = name } }, newEdits);

            //assert.
            Predicate<string[]> docsDontHaveIDButHaveName = stringDocs => stringDocs.All(s =>
            {
                var json = JObject.Parse(s);
                return json.Property(CouchDBDatabase.IdPropertyName) == null;
            });

            _db.Verify(db => db.SaveDocumentsAsync(It.Is<string[]>(strDocs => docsDontHaveIDButHaveName(strDocs)), newEdits), Times.Once);
        }

        #endregion

        #region Attachments

        public static IEnumerable<object[]> GetDataFor_SaveAttachmentAsync_Requires_Arguments()
        {
            return new List<object[]>
            {
                new object[] { null, null, null },
                new object[] { null, "something", new byte[] { 1, 2 } },
                new object[] { new SampleEntity(), null, new byte[] { 1, 2 } },
                new object[] { new SampleEntity(), null, new byte[] { 1, 2 } },
                new object[] { new SampleEntity(), string.Empty, new byte[] { 1, 2 } },
                new object[] { new SampleEntity(), "something", null },
                new object[] { new SampleEntity(), "something", new byte[] { } }
            };
        }

        [Theory]
        [MemberData(nameof(GetDataFor_SaveAttachmentAsync_Requires_Arguments))]
        public async void SaveAttachmentAsync_Requires_Arguments(IEntity entity, string attName, byte[] attachment)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAttachmentAsync(entity, attName, attachment));
        }

        [Fact]
        public async void SaveAttachmentAsync_Saves_By_Entity_Into_Db()
        {
            //arrange.
            string expectedId = "exp id", expectedRev = "exp rev";
            var expectedAttName = "some attachment name";
            var expectedAttachment = new byte[] { 1, 2, 3 };

            //act.
            await _sut.SaveAttachmentAsync(new SampleEntity { _id = expectedId, _rev = expectedRev }, expectedAttName, expectedAttachment);

            //assert.
            _db.Verify(db => db.SaveAttachmentAsync(expectedId, expectedAttName, expectedRev, expectedAttachment), Times.Once);
        }

        [Fact]
        public async void SaveAttachmentAsync_Updates_Rev_On_Success()
        {
            //arrange.
            var entity = new SampleEntity { _rev = "old rev" };
            var expectedRev = "new rev";
            _db.Setup(db => db.SaveAttachmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO { Rev = expectedRev })));

            //act.
            await _sut.SaveAttachmentAsync(entity, "attname", new byte[] { 1, 2, 3 });

            //assert.
            Assert.Equal(expectedRev, entity._rev);
        }

        #endregion
    }
}
