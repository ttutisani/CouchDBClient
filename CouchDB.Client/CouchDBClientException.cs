using System;
using System.Runtime.Serialization;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB exception.
    /// </summary>
    [Serializable]
    public sealed class CouchDBClientException : Exception
    {
        internal const string ServerResponse_Key_InSerializationInfo = "ServerResponse";

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
            ServerResponse serverResponse = null,
            Exception innerException = null)
            
            : base(message, innerException)
        {
            ServerResponse = serverResponse;
        }

        /// <summary>
        /// When overridden in a derived class, sets the System.Runtime.Serialization.SerializationInfo
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The info parameter is a null reference (Nothing in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(ServerResponse_Key_InSerializationInfo, ServerResponse);
        }
    }
}
