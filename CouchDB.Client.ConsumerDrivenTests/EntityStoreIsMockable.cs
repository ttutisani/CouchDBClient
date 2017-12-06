using Moq;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;

namespace CouchDB.Client.ConsumerDrivenTests
{
    public sealed class EntityStoreIsMockable
    {
        private readonly Mock<IEntityStore> _sut = new Mock<IEntityStore>();

        [Fact]
        public void DeleteAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.DeleteAttachmentAsync(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult<object>(null));
        }

        [Fact]
        public void DeleteEntityAsync_Is_Mockable()
        {
            _sut.Setup(s => s.DeleteEntityAsync(It.IsAny<IEntity>(), It.IsAny<bool>()))
                .Returns(Task.FromResult<object>(null));
        }

        [Fact]
        public void GetAllEntitiesAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetAllEntitiesAsync<SampleEntity>(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<SampleEntity>(0, 0, 0, new List<DocListResponseRow<SampleEntity>> { new DocListResponseRow<SampleEntity>("id", "key", new DocListResponseRowValue("revision"), new SampleEntity(), new ServerResponseError("errorString", "reason")) })));
        }

        [Fact]
        public void GetAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetAttachmentAsync(It.IsAny<IEntity>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new byte[0]));
        }

        [Fact]
        public void GetEntitiesAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetEntitiesAsync<SampleEntity>(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<SampleEntity>(0, 0, 0, new List<DocListResponseRow<SampleEntity>> { new DocListResponseRow<SampleEntity>("id", "key", new DocListResponseRowValue("revision"), new SampleEntity(), new ServerResponseError("errorString", "reason")) })));
        }

        [Fact]
        public void GetEntityAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetEntityAsync<SampleEntity>(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(new SampleEntity()));
        }

        [Fact]
        public void SaveAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveAttachmentAsync(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult<object>(null));
        }

        [Fact]
        public void SaveEntitiesAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveEntitiesAsync(It.IsAny<IEntity[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO { })));
        }

        [Fact]
        public void SaveEntityAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveEntityAsync(It.IsAny<IEntity>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult<object>(null));
        }

        #region Sample entity class

        private sealed class SampleEntity : IEntity
        {
            public string _id
            {
                get;set;
            }

            public string _rev
            {
                get;set;
            }
        }

        #endregion
    }
}
