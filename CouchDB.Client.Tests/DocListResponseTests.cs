using Newtonsoft.Json.Linq;
using System;
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
        public void FromAllDocsJson_ConvertsEachItemAsIs_WhenNotExtractingDocuments()
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
            var sut = DocListResponse<RawDocument>.FromAllDocsJson(JObject.FromObject(responseJson), extractDocumentAsObject: false);

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Length);

            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].id, sut.Rows[0]["id"]);
            Assert.Equal(responseJson.rows[0].key, sut.Rows[0]["key"]);
            Assert.NotNull(sut.Rows[0]["value"]);
            Assert.Equal(responseJson.rows[0].value.rev, sut.Rows[0]["value"]["rev"]);
            Assert.NotNull(sut.Rows[0]["doc"]);
            Assert.Equal(responseJson.rows[0].doc.someProp, sut.Rows[0]["doc"]["someProp"]);

            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].id, sut.Rows[1]["id"]);
            Assert.Equal(responseJson.rows[1].key, sut.Rows[1]["key"]);
            Assert.NotNull(sut.Rows[1]["value"]);
            Assert.Equal(responseJson.rows[1].value.rev, sut.Rows[1]["value"]["rev"]);
            Assert.NotNull(sut.Rows[1]["doc"]);
            Assert.Equal(responseJson.rows[1].doc.someProp, sut.Rows[1]["doc"]["someProp"]);
        }

        [Fact]
        public void FromCustomObjects_ConvertsEachItemDocument_WhenExtractingDocuments()
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
            var sut = DocListResponse<RawDocument.DocClass>.FromAllDocsJson(JObject.FromObject(responseJson), extractDocumentAsObject: true);

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Length);

            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].doc.someProp, sut.Rows[0]["someProp"]);

            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].doc.someProp, sut.Rows[1]["someProp"]);
        }
    }
}
