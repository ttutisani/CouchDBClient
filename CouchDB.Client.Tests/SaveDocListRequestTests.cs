using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Xunit;
using static CouchDB.Client.Tests.AssertHelper;
using System.Linq;

namespace CouchDB.Client.Tests
{
    public sealed class SaveDocListRequestTests
    {
        private readonly SaveDocListRequest _sut;

        public SaveDocListRequestTests()
        {
            _sut = new SaveDocListRequest(true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Ctor_Sets_NewEdits(bool newEdits)
        {
            //act.
            var sut = new SaveDocListRequest(newEdits);

            //assert.
            Assert.Equal(newEdits, sut.NewEdits);
        }

        private sealed class SaveDocListRequestJSON
        {
            private dynamic _request;

            public SaveDocListRequestJSON(JObject request)
            {
                _request = request;
            }

            public bool new_edits { get { return _request.new_edits; } }

            public JObject[] docs
            {
                get
                {
                    return (_request.docs as JArray).Cast<JObject>().ToArray();
                }
            }
        }

        [Fact]
        public void ToJson_Returns_Empty_Request_JSON_If_No_Items_Added()
        {
            //act.
            var json = _sut.ToJson();

            //assert.
            Assert.NotNull(json);
            var request = new SaveDocListRequestJSON(json);
            Assert.Equal(_sut.NewEdits, (bool)request.new_edits);
            Assert.Equal(0, request.docs.Length);
        }

        [Fact]
        public void AddDocuments_Expects_Array_Instance()
        {
            //act / assert.
            Assert.Throws<ArgumentNullException>(() => _sut.AddDocuments(null));
        }

        [Fact]
        public void AddDocuments_Adds_DocumentJson_Into_Request_When_Single_Document()
        {
            //arrange.
            var expectedDocument = JsonConvert.SerializeObject(new { _id = "id", _rev = "rev", _deleted = true, oneMoreProp = 123 });

            //act.
            _sut.AddDocuments(new string[] { expectedDocument });
            var json = _sut.ToJson();

            //assert.
            Assert.NotNull(json);
            var request = new SaveDocListRequestJSON(json);
            Assert.Equal(1, request.docs.Length);
            Assert.NotNull(request.docs[0]);
            Assert.True(StringIsJsonObject(expectedDocument, request.docs[0]));
        }

        [Fact]
        public void AddDocuments_Adds_DocumentJson_Into_Request_When_Multiple_Documents()
        {
            //arrange.
            var expectedDocument1 = JsonConvert.SerializeObject(new { _id = "id", _rev = "rev", _deleted = true, oneMoreProp = 123 });
            var expectedDocument2 = JsonConvert.SerializeObject(new { _id = "id", _rev = "rev", _deleted = true, anotherProp = 321, yetAnother = "whatever" });
            var expectedDocument3 = JsonConvert.SerializeObject(new { _id = "id", _rev = "rev", _deleted = true });

            //act.
            _sut.AddDocuments(new string[] { expectedDocument1, expectedDocument2, expectedDocument3 });
            var json = _sut.ToJson();

            //assert.
            Assert.NotNull(json);
            var request = new SaveDocListRequestJSON(json);
            Assert.Equal(3, request.docs.Length);

            Assert.NotNull(request.docs[0]);
            Assert.True(StringIsJsonObject(expectedDocument1, request.docs[0]));

            Assert.NotNull(request.docs[1]);
            Assert.True(StringIsJsonObject(expectedDocument2, request.docs[1]));

            Assert.NotNull(request.docs[2]);
            Assert.True(StringIsJsonObject(expectedDocument3, request.docs[2]));
        }
    }
}
