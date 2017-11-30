using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CouchDB.Client.ConsumerDrivenTests
{
    public sealed class CouchDBServerIsMockable
    {
        private readonly Mock<ICouchDBServer> _sut = new Mock<ICouchDBServer>();

        [Fact]
        public void CreateDbAsync_Is_Mockable()
        {
            _sut.Setup(s => s.CreateDbAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<object>(null));
        }

        [Fact]
        public void DeleteDbAsync_Is_Mockable()
        {
            _sut.Setup(s => s.DeleteDbAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<object>(null));
        }

        [Fact]
        public void GetAllDbNamesAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetAllDbNamesAsync(It.IsAny<ListQueryParams>()))
                .Returns(Task.FromResult(new string[0]));
        }

        [Fact]
        public void GetHandler_Is_Mockable()
        {
            _sut.Setup(s => s.GetHandler())
                .Returns(new Mock<ICouchDBHandler>().Object);
        }

        [Fact]
        public void GetInfoAsync_Is_Mockable()
        {
            _sut.Setup(s => s.GetInfoAsync())
                .Returns(Task.FromResult(new ServerInfo(new CouchDBServer.ServerInfoDTO { Vendor = new CouchDBServer.ServerInfoDTO.VendorInfoDTO() })));
        }

        [Fact]
        public void SelectDatabase_Is_Mockable()
        {
            _sut.Setup(s => s.SelectDatabase(It.IsAny<string>()))
                .Returns(new Mock<ICouchDBDatabase>().Object);

            //TODO: assert all members of ICouchDBDatabase are mockable.
        }
    }
}
