using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents collection of responses for save operation of the documents.
    /// </summary>
    public sealed class SaveDocListResponse
    {
        internal SaveDocListResponse(CouchDBDatabase.SaveDocListResponseDTO saveDocListResponseDTO)
        {
            if (saveDocListResponseDTO == null)
                throw new ArgumentNullException(nameof(saveDocListResponseDTO));

            var docResponses = saveDocListResponseDTO.Select(dto => new SaveDocResponse(dto)).ToList();
            DocumentResponses = new ReadOnlyCollection<SaveDocResponse>(docResponses);
        }

        /// <summary>
        /// Collection of responses for save operation of the documents.
        /// </summary>
        public ReadOnlyCollection<SaveDocResponse> DocumentResponses { get; }
    }
}
