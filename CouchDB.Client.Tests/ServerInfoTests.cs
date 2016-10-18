using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class ServerInfoTests
    {
        [Fact]
        public void Ctor_ShouldRequire_InfoDTO()
        {
            Assert.Throws<ArgumentNullException>(() => new ServerInfo(null));
        }

        [Fact]
        public void Ctor_InitializesProperties_WithInputDTO()
        {
            //arrange.
            var inputDTO = new CouchDBServer.ServerInfoDTO
            {
                CouchDB = "some db name",
                Version = "v 1.000.11.00",
                Vendor = new CouchDBServer.ServerInfoDTO.VendorInfoDTO
                {
                    Name = "some vendor name"
                }
            };

            //act.
            var sut = new ServerInfo(inputDTO);

            //assert.
            Assert.Equal(inputDTO.CouchDB, sut.CouchDB);
            Assert.Equal(inputDTO.Version, sut.Version);
            Assert.NotNull(sut.Vendor);
            Assert.Equal(inputDTO.Vendor.Name, sut.Vendor.Name);
        }
    }
}
