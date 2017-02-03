using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DocListResponseTests
    {
        private sealed class RawDocument
        {
            public string Id { get; set; }

            public string Key { get; set; }

            public sealed class ValueClass
            {
                public string Rev { get; set; }
            }

            public ValueClass Value { get; set; }

            public sealed class DocClass
            {
                public int SomeProp { get; set; }
            }

            public DocClass Doc { get; set; }
        }

        [Fact]
        public void FromJson_ConvertsEachItemAsIs()
        {
            //arrange.
            var responseJson = new
            {
                rows = new[] 
                {
                    new { id = "doc-123", key = "doc-223", value = new { rev = "1-aasjkxalksja" }, doc = new { someProp = 123 } },
                    new { id = "doc-123-2", key = "doc-223-2", value = new { rev = "1-aasjkxalksja-2" }, doc = new { someProp = 321 } }
                }
            };

            //act.
            var sut = DocListResponse<RawDocument>.FromJson(JObject.FromObject(responseJson));

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Count);

            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].id, sut.Rows[0].Id);
            Assert.Equal(responseJson.rows[0].key, sut.Rows[0].Key);
            Assert.NotNull(sut.Rows[0].Value);
            Assert.Equal(responseJson.rows[0].value.rev, sut.Rows[0].Value.Revision);
            Assert.NotNull(sut.Rows[0].Document);
            Assert.Equal(responseJson.rows[0].doc.someProp, sut.Rows[0].Document["someProp"]);

            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].id, sut.Rows[1].Id);
            Assert.Equal(responseJson.rows[1].key, sut.Rows[1].Key);
            Assert.NotNull(sut.Rows[1].Value);
            Assert.Equal(responseJson.rows[1].value.rev, sut.Rows[1].Value.Revision);
            Assert.NotNull(sut.Rows[1].Document);
            Assert.Equal(responseJson.rows[1].doc.someProp, sut.Rows[1].Document["someProp"]);
        }
        
    }
}
