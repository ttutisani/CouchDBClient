using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DbResponseTests
    {
        [Fact]
        public void Ctor_RequiresDTO()
        {
            //act / assert.
            Assert.Throws<ArgumentNullException>(() => new DocumentResponse(null));
        }

        [Fact]
        public void Ctor_Initializes_IdAndRev()
        {
            //arrange.
            var dto = new CouchDBDatabase.DocumentResponseDTO { Id = "some id", Rev = "some rev" };

            //act.
            var sut = new DocumentResponse(dto);

            //assert.
            Assert.Equal(dto.Id, sut.Id);
            Assert.Equal(dto.Rev, sut.Revision);
        }
    }
}
