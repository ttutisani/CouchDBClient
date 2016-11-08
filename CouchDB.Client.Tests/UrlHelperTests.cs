using System;
using Xunit;

namespace CouchDB.Client.Tests
{
    public sealed class UrlHelperTests
    {
        [Theory]
        [InlineData("url1", null, "url1")]
        [InlineData(null, "url2", "url2")]
        public void CombineUrl_ReturnsNotNull_IfAnyNull(string prefix, string suffix, string expected)
        {
            //act.
            var combined = UrlHelper.CombineUrl(prefix, suffix);

            //assert.
            Assert.Equal(expected, combined);
        }

        [Theory]
        [InlineData(" ", "url")]
        [InlineData("url", " ")]
        public void CombineUrl_DoesNotAccept_WhitespacePrefixOrSuffix(string prefix, string suffix)
        {
            //act / assert.
            Assert.Throws<ArgumentException>(() => UrlHelper.CombineUrl(prefix, suffix));
        }

        [Theory]
        [InlineData("prefix", "suffix", "prefix/suffix")]
        [InlineData("prefix/", "/suffix", "prefix/suffix")]
        [InlineData("prefix/", "suffix", "prefix/suffix")]
        [InlineData("prefix", "/suffix", "prefix/suffix")]
        public void CombineUrl_CombinesAllKindsOfUrl(string prefix, string suffix, string expected)
        {
            //act.
            var combined = UrlHelper.CombineUrl(prefix, suffix);

            //assert.
            Assert.Equal(expected, combined);
        }
    }
}
