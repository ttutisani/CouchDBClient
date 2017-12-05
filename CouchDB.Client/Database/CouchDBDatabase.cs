using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents signle database within CouchDB server instance.
    /// </summary>
    public sealed class CouchDBDatabase : ICouchDBDatabase, IDisposable
    {
        internal const string IdPropertyName = "_id";
        internal const string RevisionPropertyName = "_rev";

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBDatabase"/> class.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="FormatException">URL is not in valid format.</exception>
        internal CouchDBDatabase(string baseUrl)
            : this(new CouchDBHandler(baseUrl))
        {
        }

        private readonly ICouchDBHandler _handler;

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBDatabase"/> class.
        /// </summary>
        /// <param name="handler"></param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        internal CouchDBDatabase(ICouchDBHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handler = handler;
        }

        /// <summary>
        /// Disposes current instance of <see cref="CouchDBDatabase"/>, after which it becomes unusable.
        /// </summary>
        public void Dispose()
        {
            var disposableHandler = _handler as IDisposable;
            disposableHandler?.Dispose();
        }

        #region Save doc

        /// <summary>
        /// Represents response of document operation.
        /// </summary>
        public sealed class SaveDocResponseDTO
        {
            /// <summary>
            /// Gets or sets document id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets document revision.
            /// </summary>
            public string Rev { get; set; }

            /// <summary>
            /// Gets or sets raw error string.
            /// </summary>
            public string Error { get; set; }

            /// <summary>
            /// Gets or sets reason phrase.
            /// </summary>
            public string Reason { get; set; }
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<SaveDocResponse> SaveStringDocumentAsync(string documentJsonString, DocUpdateParams updateParams = null)
        {
            if (string.IsNullOrWhiteSpace(documentJsonString))
                throw new ArgumentNullException(nameof(documentJsonString));

            var newDocUrl = QueryParams.AppendQueryParams(string.Empty, updateParams);
            var response = await _handler.SendRequestAsync(newDocUrl, RequestMethod.POST, Request.JsonString(documentJsonString)).Safe();
            if (response == null)
                return null;

            var docResponseDTO = await response.ReadAsAsync<SaveDocResponseDTO>(false).Safe();
            return new SaveDocResponse(docResponseDTO);
        }
        
        #endregion

        #region Get doc

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns><see cref="string"/> containing document JSON.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<string> GetStringDocumentAsync(string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var docQuery = QueryParams.AppendQueryParams(docId, queryParams);
            var response = await _handler.SendRequestAsync(docQuery, RequestMethod.GET, Request.Empty).Safe();
            if (response == null)
                return null;

            return await response.ReadAsStringAsync(true).Safe();
        }
        
        #endregion

        #region Delete doc

        /// <summary>
        /// Marks the specified document as deleted by adding a field 
        /// _deleted with the value true. 
        /// Documents with this field will not be returned within requests anymore, 
        /// but stay in the database. 
        /// You must supply the current (latest) revision, by using the <paramref name="revision"/> parameter.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="revision">Actual document’s revision.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<SaveDocResponse> DeleteDocumentAsync(string docId, string revision, bool batch = false)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentNullException(nameof(revision));

            var deleteDocParams = new DeleteDocParams
            {
                Revision = revision,
                Batch = batch
            };
            var deleteDocUrl = QueryParams.AppendQueryParams(docId, deleteDocParams);
            var response = await _handler.SendRequestAsync(deleteDocUrl, RequestMethod.DELETE, Request.Empty).Safe();
            if (response == null)
                return null;

            var responseDTO = await response.ReadAsAsync<SaveDocResponseDTO>(true).Safe()
                ?? new SaveDocResponseDTO { Id = docId, Rev = revision };

            return new SaveDocResponse(responseDTO);
        }
        
        #endregion

        #region Get all docs

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{STRING}"/> containing list of JSON strings.</returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public async Task<DocListResponse<string>> GetAllStringDocumentsAsync(ListQueryParams queryParams = null)
        {
            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);
            var response = await _handler.SendRequestAsync(allDocsUrl, RequestMethod.GET, Request.Empty).Safe();
            if (response == null)
                return null;

            var allDocsJsonString = await response.ReadAsStringAsync(false).Safe();
            var docListResponse = DocListResponse<string>.FromJsonToStringList(allDocsJsonString);
            return docListResponse;
        }

        #endregion

        #region Get Docs

        /// <summary>
        /// Returns a JSON structure of the documents in a given database, found by ID list. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="docIdList">Array of document IDs to be retrieved.</param>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{STRING}"/> containing list of JSON strings.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public async Task<DocListResponse<string>> GetStringDocumentsAsync(string[] docIdList, ListQueryParams queryParams = null)
        {
            if (docIdList == null || docIdList.Length == 0)
                throw new ArgumentNullException(nameof(docIdList));

            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);
            var allDocsRequest = new { keys = docIdList };
            var allDocsJsonRequest = JsonConvert.SerializeObject(allDocsRequest);

            var allDocsResponse = await _handler.SendRequestAsync(allDocsUrl, RequestMethod.POST, Request.JsonString(allDocsJsonRequest)).Safe();
            if (allDocsResponse == null)
                return null;

            var allDocsJsonString = await allDocsResponse.ReadAsStringAsync(false).Safe();
            var docListResponse = DocListResponse<string>.FromJsonToStringList(allDocsJsonString);
            return docListResponse;
        }

        #endregion

        #region Save Docs

        /// <summary>
        /// Represents collection of responses for save operation of the documents.
        /// </summary>
        public sealed class SaveDocListResponseDTO : List<SaveDocResponseDTO>
        {
        }

        /// <summary>
        /// Allows you to create and update multiple documents at the same time within a single request. The basic operation is similar to creating or updating a single document, except that you batch the document structure and information.
        /// When creating new documents the document ID (_id) is optional.
        /// For updating existing documents, you must provide the document ID, revision information (_rev), and new document values.
        /// In case of batch deleting documents all fields as document ID, revision information and deletion status (_deleted) are required.
        /// </summary>
        /// <param name="documents">List of documents strings.</param>
        /// <param name="newEdits">If false, prevents the database from assigning them new revision IDs. Default is true. Optional</param>
        /// <returns>Instance of <see cref="SaveDocListResponse"/> with detailed information for each requested document to save.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<SaveDocListResponse> SaveStringDocumentsAsync(string[] documents, bool newEdits = true)
        {
            if (documents == null || documents.Length == 0)
                throw new ArgumentNullException(nameof(documents));

            var saveDocListRequest = new SaveDocListRequest(newEdits);
            saveDocListRequest.AddDocuments(documents);
            var saveDocListRequestJson = saveDocListRequest.ToJson().ToString();

            var saveDocListResponse = await _handler.SendRequestAsync("_bulk_docs", RequestMethod.POST, Request.JsonString(saveDocListRequestJson)).Safe();
            if (saveDocListResponse == null)
                return null;

            var saveDocListResponseDTO = await saveDocListResponse.ReadAsAsync<SaveDocListResponseDTO>(false).Safe();

            return new SaveDocListResponse(saveDocListResponseDTO);
        }

        #endregion

        #region Attachments

        /// <summary>
        /// Uploads the supplied content as an attachment to the specified document.
        /// </summary>
        /// <param name="docId">Document ID</param>
        /// <param name="attName">Attachment name</param>
        /// <param name="revision">Document revision. Required.</param>
        /// <param name="attachment">Attachment content.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<SaveDocResponse> SaveAttachmentAsync(string docId, string attName, string revision, byte[] attachment)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(attName))
                throw new ArgumentNullException(nameof(attName));

            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentNullException(nameof(revision));

            var queryParams = new AttachmentQueryParams { Rev = revision };

            var saveAttUrl = QueryParams.AppendQueryParams($"{docId}/{attName}", queryParams);
            var saveAttResponse = await _handler.SendRequestAsync(saveAttUrl, RequestMethod.PUT, Request.Raw(attachment)).Safe();
            if (saveAttResponse == null)
                return null;

            var saveAttResponseDTO = await saveAttResponse.ReadAsAsync<SaveDocResponseDTO>(false).Safe();
            return new SaveDocResponse(saveAttResponseDTO);
        }

        /// <summary>
        /// Returns the file attachment associated with the document. 
        /// The raw data of the associated attachment is returned (just as if you were accessing a static file).
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="attName">Attachment name.</param>
        /// <returns>Attachment content.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<byte[]> GetAttachmentAsync(string docId, string attName)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(attName))
                throw new ArgumentNullException(nameof(attName));

            var attachmentUrl = $"{docId}/{attName}";
            var attachmentResponse = await _handler.SendRequestAsync(attachmentUrl, RequestMethod.GET, Request.Empty).Safe();
            if (attachmentResponse == null)
                return null;

            var attachmentBytes = await attachmentResponse.ReadAsByteArrayAsync(true).Safe();

            return attachmentBytes;
        }

        /// <summary>
        /// Deletes the attachment of the specified doc.
        /// You must supply the current revision to delete the attachment.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="attName">Attachment name.</param>
        /// <param name="revision">Document revision. Required.</param>
        /// <param name="batch">Store changes in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<SaveDocResponse> DeleteAttachmentAsync(string docId, string attName, string revision, bool batch = false)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(attName))
                throw new ArgumentNullException(nameof(attName));

            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentNullException(nameof(revision));

            var deleteAttParams = new DeleteDocParams
            {
                Revision = revision,
                Batch = batch
            };

            var deleteAttUrl = QueryParams.AppendQueryParams($"{docId}/{attName}", deleteAttParams);
            var deleteAttresponse = await _handler.SendRequestAsync(deleteAttUrl, RequestMethod.DELETE, Request.Empty).Safe();
            if (deleteAttresponse == null)
                return null;

            var deleteAttResponseDTO = await deleteAttresponse.ReadAsAsync<SaveDocResponseDTO>(false).Safe();

            return new SaveDocResponse(deleteAttResponseDTO);
        }

        #endregion
    }
}
