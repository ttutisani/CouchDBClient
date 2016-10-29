using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB exception.
    /// </summary>
    public sealed class CouchDBClientException : Exception
    {
        /// <summary>
        /// Gets <see cref="ServerResponse"/> which represents JSON object returned by CouchDB server.
        /// </summary>
        public ServerResponse ServerResponse { get; }

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBClientException"/> class.
        /// </summary>
        /// <param name="message">Error message which describes this exception.</param>
        /// <param name="serverResponse">(optional) instance of <see cref="ServerResponse"/> received from server.</param>
        /// <param name="innerException">Inner exception describing more specific error.</param>
        public CouchDBClientException(
            string message, 
            ServerResponse serverResponse,
            Exception innerException = null)
            
            : base(message, innerException)
        {
            ServerResponse = serverResponse;
        }
    }
}
