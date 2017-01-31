using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents JSON object received from server in response to a request.
    /// </summary>
    public sealed class ServerResponse
    {
        /// <summary>
        /// Gets <see cref="bool"/> specifying success of the operation.
        /// </summary>
        public bool OK { get; }

        /// <summary>
        /// Gets <see cref="ServerResponseError"/> describing what kind of error happened.
        /// </summary>
        public ServerResponseError Error { get; }

        /// <summary>
        /// Gets reason text.
        /// </summary>
        public string Reason { get; }

        internal ServerResponse(CouchDBServer.ServerResponseDTO serverResponseDTO)
        {
            if (serverResponseDTO == null)
                throw new ArgumentNullException(nameof(serverResponseDTO));

            OK = serverResponseDTO.OK;
            Error = serverResponseDTO.Error == null ? null : new ServerResponseError(serverResponseDTO.Error);
            Reason = serverResponseDTO.Reason;
        }
    }
}
