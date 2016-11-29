using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class DocListResponseTests
    {
        [Fact]
        public void Ctor_Requires_JsonObject()
        {
            //arrange / act / assert.
            Assert.Throws<ArgumentNullException>(() => DocListResponse<JObject>.FromJsonObjects(null));
        }

        [Fact]
        public void Ctor_InitializesSimpleProperties_FromResponseJson()
        {
            //arrange.
            var responseJson = new { offset = 12, total_rows = 23, update_seq = 34 };

            //act.
            var sut = DocListResponse<JObject>.FromJsonObjects(JObject.FromObject(responseJson));

            //assert.
            Assert.Equal(responseJson.offset, sut.Offset);
            Assert.Equal(responseJson.total_rows, sut.TotalRows);
            Assert.Equal(responseJson.update_seq, sut.UpdateSeq);
        }

        [Fact]
        public void Ctor_PutsRowsArray_AsPassed()
        {
            //arrange.
            var responseJson = new { rows = new[] { new { a = 123 }, new { a = 321 } } };

            //act.
            var sut = DocListResponse<JObject>.FromJsonObjects(JObject.FromObject(responseJson));

            //assert.
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Count);
            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].a, sut.Rows[0]["a"].Value<int>());
            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].a, sut.Rows[1]["a"].Value<int>());
        }

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
        public void FromCustomObjects_ConvertsEachItemAsIs_WhenNotExtractingDocuments()
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
            var sut = DocListResponse<RawDocument>.FromCustomObjects<RawDocument>(JObject.FromObject(responseJson), extractDocumentAsObject: false);

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Count);

            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].id, sut.Rows[0].Id);
            Assert.Equal(responseJson.rows[0].key, sut.Rows[0].Key);
            Assert.NotNull(sut.Rows[0].Value);
            Assert.Equal(responseJson.rows[0].value.rev, sut.Rows[0].Value.Rev);
            Assert.NotNull(sut.Rows[0].Doc);
            Assert.Equal(responseJson.rows[0].doc.someProp, sut.Rows[0].Doc.SomeProp);

            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].id, sut.Rows[1].Id);
            Assert.Equal(responseJson.rows[1].key, sut.Rows[1].Key);
            Assert.NotNull(sut.Rows[1].Value);
            Assert.Equal(responseJson.rows[1].value.rev, sut.Rows[1].Value.Rev);
            Assert.NotNull(sut.Rows[1].Doc);
            Assert.Equal(responseJson.rows[1].doc.someProp, sut.Rows[1].Doc.SomeProp);
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
            var sut = DocListResponse<RawDocument.DocClass>.FromCustomObjects<RawDocument.DocClass>(JObject.FromObject(responseJson), extractDocumentAsObject: true);

            //assert.
            Assert.NotNull(sut);
            Assert.NotNull(sut.Rows);
            Assert.Equal(responseJson.rows.Length, sut.Rows.Count);

            Assert.NotNull(sut.Rows[0]);
            Assert.Equal(responseJson.rows[0].doc.someProp, sut.Rows[0].SomeProp);

            Assert.NotNull(sut.Rows[1]);
            Assert.Equal(responseJson.rows[1].doc.someProp, sut.Rows[1].SomeProp);
        }
    }
}
