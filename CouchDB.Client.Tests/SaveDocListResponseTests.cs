using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class SaveDocListResponseTests
    {
        [Fact]
        public void Ctor_Requires_DTO()
        {
            Assert.Throws<ArgumentNullException>(() => new SaveDocListResponse(null));
        }

        [Fact]
        public void Ctor_Keeps_Empty_DocumentResponses_If_No_DocDTO_Provided()
        {
            //arrange.
            var dto = new CouchDBDatabase.SaveDocListResponseDTO();

            //act.
            var sut = new SaveDocListResponse(dto);

            //assert.
            Assert.NotNull(sut.DocumentResponses);
            Assert.Empty(sut.DocumentResponses);
        }

        [Fact]
        public void Ctor_Puts_All_DTOs_Into_DocumentResponses()
        {
            //arrange.
            var dto = new CouchDBDatabase.SaveDocListResponseDTO
            {
                new CouchDBDatabase.SaveDocResponseDTO { Id = "id1", Rev = "rev1" },
                new CouchDBDatabase.SaveDocResponseDTO { Id = "id2", Rev = "rev2" },
                new CouchDBDatabase.SaveDocResponseDTO { Id = "id3", Rev = "rev3" }
            };

            //act.
            var sut = new SaveDocListResponse(dto);

            //assert.
            Assert.NotNull(sut.DocumentResponses);
            Assert.Equal(3, sut.DocumentResponses.Count);

            Assert.Equal(dto[0].Id, sut.DocumentResponses[0].Id);
            Assert.Equal(dto[0].Rev, sut.DocumentResponses[0].Revision);

            Assert.Equal(dto[1].Id, sut.DocumentResponses[1].Id);
            Assert.Equal(dto[1].Rev, sut.DocumentResponses[1].Revision);

            Assert.Equal(dto[2].Id, sut.DocumentResponses[2].Id);
            Assert.Equal(dto[2].Rev, sut.DocumentResponses[2].Revision);
        }
    }
}
