using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents response of document operation.
    /// </summary>
    public sealed class SaveDocResponse
    {
        internal SaveDocResponse(CouchDBDatabase.DocumentResponseDTO docResponseDTO)
        {
            if (docResponseDTO == null)
                throw new ArgumentNullException(nameof(docResponseDTO));

            Id = docResponseDTO.Id;
            Revision = docResponseDTO.Rev;
        }

        /// <summary>
        /// Gets document id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets document revision.
        /// </summary>
        public string Revision { get; }
    }
}
