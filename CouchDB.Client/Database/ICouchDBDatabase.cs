using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Base interface representing core capabilities of Couch DB Database.
    /// </summary>
    public interface ICouchDBDatabase
    {
        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<SaveDocResponse> SaveStringDocumentAsync(string documentJsonString, DocUpdateParams updateParams = null);

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns><see cref="string"/> containing document JSON.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<string> GetStringDocumentAsync(string docId, DocQueryParams queryParams = null);

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
        Task<SaveDocResponse> DeleteDocumentAsync(string docId, string revision, bool batch = false);

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
        Task<DocListResponse<string>> GetAllStringDocumentsAsync(ListQueryParams queryParams = null);

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
        Task<DocListResponse<string>> GetStringDocumentsAsync(string[] docIdList, ListQueryParams queryParams = null);

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
        Task<SaveDocListResponse> SaveStringDocumentsAsync(string[] documents, bool newEdits = true);

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
        Task<SaveDocResponse> SaveAttachmentAsync(string docId, string attName, string revision, byte[] attachment);

        /// <summary>
        /// Returns the file attachment associated with the document. 
        /// The raw data of the associated attachment is returned (just as if you were accessing a static file).
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="attName">Attachment name.</param>
        /// <returns>Attachment content.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<byte[]> GetAttachmentAsync(string docId, string attName);

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
        Task<SaveDocResponse> DeleteAttachmentAsync(string docId, string attName, string revision, bool batch = false);
    }
}
