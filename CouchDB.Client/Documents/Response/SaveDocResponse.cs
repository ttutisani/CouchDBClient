using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents response of document operation.
    /// </summary>
    public sealed class SaveDocResponse
    {
        internal SaveDocResponse(CouchDBDatabase.SaveDocResponseDTO docResponseDTO)
        {
            if (docResponseDTO == null)
                throw new ArgumentNullException(nameof(docResponseDTO));

            Id = docResponseDTO.Id;
            Revision = docResponseDTO.Rev;

            Error = ServerResponseError.FromString(docResponseDTO.Error, docResponseDTO.Reason);
        }

        /// <summary>
        /// Gets document id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets document revision.
        /// </summary>
        public string Revision { get; }

        /// <summary>
        /// Error related information, if any.
        /// </summary>
        public ServerResponseError Error { get; }
    }
}
