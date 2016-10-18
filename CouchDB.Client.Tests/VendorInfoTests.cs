using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class VendorInfoTests
    {
        [Fact]
        public void Ctor_Requires_VendorInfoDTO()
        {
            Assert.Throws<ArgumentNullException>(() => new VendorInfo(null));
        }

        [Fact]
        public void Ctor_InitializesInstance_WithInputDTO()
        {
            //arrange.
            var inputDTO = new CouchDBServer.ServerInfoDTO.VendorInfoDTO
            {
                Name = "vendor name here"
            };

            //act.
            var sut = new VendorInfo(inputDTO);

            //assert.
            Assert.Equal(inputDTO.Name, sut.Name);
        }
    }
}
