using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Extension methods (overloads) over <see cref="ICouchDBDatabase"/> interface.
    /// </summary>
    public static class CouchDBDatabaseExtensions
    {
        #region Save doc

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="documentJsonObject">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public static async Task SaveDocumentAsync(this ICouchDBDatabase @this, JObject documentJsonObject, DocUpdateParams updateParams = null)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));
            
            var saveResponse = await @this.SaveDocumentAsync(documentJsonObject.ToString(), updateParams).Safe();
            documentJsonObject[CouchDBDatabase.IdPropertyName] = saveResponse.Id;
            documentJsonObject[CouchDBDatabase.RevisionPropertyName] = saveResponse.Revision;
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="documentObject">Document object to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public static async Task<SaveDocResponse> SaveDocumentAsync(this ICouchDBDatabase @this, object documentObject, DocUpdateParams updateParams = null)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            var documentJsonObject = JObject.FromObject(documentObject);
            if (string.IsNullOrWhiteSpace(documentJsonObject[CouchDBDatabase.IdPropertyName]?.ToString()))
                documentJsonObject.Remove(CouchDBDatabase.IdPropertyName);

            return await @this.SaveDocumentAsync(documentJsonObject.ToString(), updateParams).Safe();
        }

        #endregion

        #region Get doc

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns><see cref="JObject"/> containing document JSON.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public static async Task<JObject> GetDocumentJsonAsync(this ICouchDBDatabase @this, string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var jsonString = await @this.GetDocumentAsync(docId, queryParams).Safe();
            var jsonObject = jsonString != null
                ? JObject.Parse(jsonString)
                : null;

            return jsonObject;
        }

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <typeparam name="TResult">Specify type to which the document will be deserialized.</typeparam>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns>Object containing deserialized document.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public static async Task<TResult> GetDocumentAsync<TResult>(this ICouchDBDatabase @this, string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var jsonString = await @this.GetDocumentAsync(docId, queryParams).Safe();
            var resultObject = jsonString != null
                ? JsonConvert.DeserializeObject<TResult>(jsonString)
                : default(TResult);

            return resultObject;
        }

        #endregion

        #region Delete doc

        /// <summary>
        /// Marks the specified document as deleted by adding a field 
        /// _deleted with the value true. 
        /// Documents with this field will not be returned within requests anymore, 
        /// but stay in the database. 
        /// You must supply the current (latest) revision, by using the "_rev" property.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="document"><see cref="JObject"/> instance representing a document.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        public static async Task DeleteDocumentAsync(this ICouchDBDatabase @this, JObject document, bool batch = false)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var docId = document[CouchDBDatabase.IdPropertyName]?.ToString();
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentException("Document should have _id.", nameof(document));

            var revision = document[CouchDBDatabase.RevisionPropertyName]?.ToString();
            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentException("Document shoudl have _rev.", nameof(document));

            var deletionResponse = await @this.DeleteDocumentAsync(docId, revision, batch).Safe();
            document[CouchDBDatabase.IdPropertyName] = deletionResponse.Id;
            document[CouchDBDatabase.RevisionPropertyName] = deletionResponse.Revision;
        }

        #endregion

        #region Get all docs

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <typeparam name="TDocument">Specifies resulting document object type.</typeparam>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="extractDocumentAsObject">Boolean indicating whether to extract document portion of the 
        /// JSON as object. If False, then the whole JSON is deserialized as object, instead of extracting the 
        /// document portion only.</param>
        /// <param name="deserializer">Provide your own deserializer if you prefer. 
        /// By default, it will deserialize by using NewtonSoft.Json methods.
        /// NOTE: if the specified <typeparamref name="TDocument"/> does not have parameterless constructor,
        /// you should specify the deserializer as well. Otherwise, runtime exception will be thrown.</param>
        /// <returns><see cref="DocListResponse{TDOcument}"/> containing list of JSON objects (<typeparamref name="TDocument"/>).</returns>
        /// <exception cref="ArgumentException"><paramref name="extractDocumentAsObject"/> can be true only when Include_Docs is true within <paramref name="queryParams"/>.</exception>
        public static async Task<DocListResponse<TDocument>> GetAllObjectDocumentsAsync<TDocument>(this ICouchDBDatabase @this, ListQueryParams queryParams = null, bool extractDocumentAsObject = false, Func<JObject, TDocument> deserializer = null)
        {
            var jsonDocs = await @this.GetAllJsonDocumentsAsync(queryParams, extractDocumentAsObject).Safe();

            return jsonDocs.Cast(deserializer ?? new Func<JObject, TDocument>(json => json.ToObject<TDocument>()));
        }

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="extractDocumentAsObject">Boolean indicating whether to extract document portion of the 
        /// JSON as object. If False, then the whole JSON is deserialized as object, instead of extracting the 
        /// document portion only.</param>
        /// <returns><see cref="DocListResponse{STRING}"/> containing list of JSON strings.</returns>
        public static async Task<DocListResponse<string>> GetAllStringDocumentsAsync(this ICouchDBDatabase @this, ListQueryParams queryParams = null, bool extractDocumentAsObject = false)
        {
            var jsonDocs = await @this.GetAllJsonDocumentsAsync(queryParams, extractDocumentAsObject).Safe();

            return jsonDocs.Cast(json => json.ToString());
        }

        #endregion
    }
}
