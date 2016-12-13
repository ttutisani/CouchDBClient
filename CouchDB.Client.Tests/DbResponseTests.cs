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
            Assert.Throws<ArgumentNullException>(() => new SaveDocResponse(null));
        }

        [Fact]
        public void Ctor_Initializes_IdAndRev()
        {
            //arrange.
            var dto = new CouchDBDatabase.SaveDocResponseDTO { Id = "some id", Rev = "some rev" };

            //act.
            var sut = new SaveDocResponse(dto);

            //assert.
            Assert.Equal(dto.Id, sut.Id);
            Assert.Equal(dto.Rev, sut.Revision);
        }
    }
}
