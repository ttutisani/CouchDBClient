using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class CommonErrorTests
    {
        [Theory]
        [InlineData(CommonError.Bad_Content_Type, "bad_content_type")]
        [InlineData(CommonError.Bad_Request, "bad_request")]
        [InlineData(CommonError.Conflict, "conflict")]
        [InlineData(CommonError.File_Exists, "file_exists")]
        [InlineData(CommonError.Illegal_DocId, "illegal_docid")]
        [InlineData(CommonError.Not_Found, "not_found")]
        public void ToErrorString_Returns_LowerCase_ErrorName(CommonError input, string output)
        {
            //act.
            var error = input.ToErrorString();

            //assert.
            Assert.Equal(output, error);
        }

        [Theory]
        [InlineData(CommonError.Bad_Content_Type, "bad_content_type", true)]
        [InlineData(CommonError.Bad_Content_Type, "BAD_CONTENT_TYPE", true)]
        [InlineData(CommonError.Bad_Content_Type, "bad_content_type_", false)]
        [InlineData(CommonError.Bad_Content_Type, "", false)]
        [InlineData(CommonError.Bad_Content_Type, null, false)]
        public void EqualsErrorString_Compares_With_Parsed_String(CommonError input, string errorString, bool shouldEqual)
        {
            //act.
            var equals = input.EqualsErrorString(errorString);

            //assert.
            Assert.Equal(shouldEqual, equals);
        }
    }
}
