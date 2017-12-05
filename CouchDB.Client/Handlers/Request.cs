using System.Net.Http;
using System.Text;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents request sent to CouchDB.
    /// This class cannot be instantiated (it's abstract).
    /// Instances of it can be obtained from static members of this class (e.g. <see cref="Empty"/>),
    /// or by inheriting it.
    /// </summary>
    public abstract class Request
    {
        /// <summary>
        /// Empty request content.
        /// </summary>
        public static readonly Request Empty;

        /// <summary>
        /// Converts given <see cref="Request"/> instance into <see cref="HttpContent"/> instance.
        /// </summary>
        /// <returns></returns>
        public abstract HttpContent ToHttpContent();

        #region String

        /// <summary>
        /// Generates <see cref="Request"/> with json string content in it.
        /// </summary>
        /// <param name="content">Json string.</param>
        /// <returns>Instance of <see cref="Request"/>.</returns>
        /// <exception cref="NoException"></exception>
        public static Request JsonString(string content) 
            => 
            new StringRequest(content, Encoding.UTF8, "application/json");

        private sealed class StringRequest : Request
        {
            private readonly HttpContent _content;

            public StringRequest(string content, Encoding encoding, string mediaType)
            {
                _content = new StringContent(content, encoding, mediaType);
            }

            public override HttpContent ToHttpContent()
            {
                return _content;
            }
        }

        #endregion

        #region Raw

        /// <summary>
        /// Generates <see cref="Request"/> with row byte array content in it.
        /// </summary>
        /// <param name="content">Byte array content.</param>
        /// <returns>Instance of <see cref="Request"/>.</returns>
        public static Request Raw(byte[] content) => new RawRequest(content);

        private sealed class RawRequest : Request
        {
            private readonly HttpContent _httpContent;

            public RawRequest(byte[] attachment)
            {
                _httpContent = new ByteArrayContent(attachment);
            }

            public override HttpContent ToHttpContent()
            {
                return _httpContent;
            }
        }

        #endregion
    }
}
