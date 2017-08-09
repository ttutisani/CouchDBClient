using System.Net.Http;

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
        /// Represents empty request content.
        /// </summary>
        public static readonly Request Empty;

        /// <summary>
        /// Converts given <see cref="Request"/> instance into <see cref="HttpContent"/> instance.
        /// </summary>
        /// <returns></returns>
        public abstract HttpContent ToHttpContent();
    }
}
