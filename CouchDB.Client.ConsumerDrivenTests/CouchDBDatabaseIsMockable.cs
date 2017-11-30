using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.ConsumerDrivenTests
{
    public sealed class CouchDBDatabaseIsMockable
    {
        private readonly Mock<ICouchDBDatabase> _sut = new Mock<ICouchDBDatabase>();

        [Fact]
        public void SaveStringDocumentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveStringDocumentAsync(It.IsAny<string>(), It.IsAny<DocUpdateParams>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));
        }

        [Fact]
        public void GetStringDocumentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetStringDocumentAsync(It.IsAny<string>(), It.IsAny<DocQueryParams>()))
                .Returns(Task.FromResult(string.Empty));
        }

        [Fact]
        public void DeleteDocumentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.DeleteDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));
        }

        [Fact]
        public void GetAllStringDocumentsAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetAllStringDocumentsAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 0, 0, new List<DocListResponseRow<string>> { new DocListResponseRow<string>("id", "key", new DocListResponseRowValue("revision"), "doc", new ServerResponseError("error", "reason")) })));
        }

        [Fact]
        public void GetStringDocumentsAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetStringDocumentsAsync(It.IsAny<string[]>(), It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new DocListResponse<string>(0, 0, 0, new List<DocListResponseRow<string>> { new DocListResponseRow<string>("id", "key", new DocListResponseRowValue("revision"), "doc", new ServerResponseError("error", "reason")) })));
        }

        [Fact]
        public void SaveStringDocumentsAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveStringDocumentsAsync(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocListResponse(new CouchDBDatabase.SaveDocListResponseDTO { new CouchDBDatabase.SaveDocResponseDTO() })));
        }

        [Fact]
        public void SaveAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.SaveAttachmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));
        }

        [Fact]
        public void GetAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetAttachmentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new byte[0]));
        }

        [Fact]
        public void DeleteAttachmentAsync_Is_Mockable()
        {
            _sut.Setup(s => s.DeleteAttachmentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new SaveDocResponse(new CouchDBDatabase.SaveDocResponseDTO())));
        }
    }
}
