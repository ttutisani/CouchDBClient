using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class SaveDocResponseTests
    {
        [Fact]
        public void Ctor_RequiresDTO()
        {
            //act / assert.
            Assert.Throws<ArgumentNullException>(() => new SaveDocResponse(null));
        }

        [Fact]
        public void Ctor_Initializes_Id_And_Rev()
        {
            //arrange.
            var dto = new CouchDBDatabase.SaveDocResponseDTO { Id = "some id", Rev = "some rev" };

            //act.
            var sut = new SaveDocResponse(dto);

            //assert.
            Assert.Equal(dto.Id, sut.Id);
            Assert.Equal(dto.Rev, sut.Revision);
            Assert.Null(sut.Error);
        }

        [Fact]
        public void Ctor_Initializes_Error_When_Passed()
        {
            //arrange.
            var dto = new CouchDBDatabase.SaveDocResponseDTO { Error = "some error", Reason = "some reason" };

            //act.
            var sut = new SaveDocResponse(dto);

            //assert.
            Assert.NotNull(sut.Error);
            Assert.Equal(dto.Error, sut.Error.RawError);
            Assert.Equal(dto.Reason, sut.Error.Reason);
        }
    }
}
