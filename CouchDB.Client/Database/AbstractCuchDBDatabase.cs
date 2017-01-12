using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Base class for <see cref="CouchDBDatabase"/> which defines all operations on CouchDB Database.
    /// </summary>
    public abstract class AbstractCuchDBDatabase
    {
        internal const string IdPropertyName = "_id";
        internal const string RevisionPropertyName = "_rev";

        #region Save doc

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public abstract Task<SaveDocResponse> SaveDocumentAsync(string documentJsonString, DocUpdateParams updateParams = null);

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonObject">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task SaveDocumentAsync(JObject documentJsonObject, DocUpdateParams updateParams = null)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));

            if (string.IsNullOrWhiteSpace(documentJsonObject[IdPropertyName]?.ToString()))
                documentJsonObject.Remove(IdPropertyName);

            var saveResponse = await SaveDocumentAsync(documentJsonObject.ToString(), updateParams);
            documentJsonObject[IdPropertyName] = saveResponse.Id;
            documentJsonObject[RevisionPropertyName] = saveResponse.Revision;
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentObject">Document object to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<SaveDocResponse> SaveDocumentAsync(object documentObject, DocUpdateParams updateParams = null)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            return await SaveDocumentAsync(JsonConvert.SerializeObject(documentObject), updateParams);
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
        public abstract Task<string> GetDocumentAsync(string docId, DocQueryParams queryParams = null);

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns><see cref="JObject"/> containing document JSON.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<JObject> GetDocumentJsonAsync(string docId, DocQueryParams queryParams = null)
        {
            var jsonString = await GetDocumentAsync(docId, queryParams);
            var jsonObject = jsonString != null
                ? JObject.Parse(jsonString)
                : null;

            return jsonObject;
        }

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <typeparam name="TResult">Specify type to which the document will be deserialized.</typeparam>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns>Object containing deserialized document.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<TResult> GetDocumentAsync<TResult>(string docId, DocQueryParams queryParams = null)
        {
            var jsonString = await GetDocumentAsync(docId, queryParams);
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
        /// You must supply the current (latest) revision, by using the <paramref name="revision"/> parameter.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="revision">Actual document’s revision.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        public abstract Task<SaveDocResponse> DeleteDocumentAsync(string docId, string revision, bool batch = false);

        /// <summary>
        /// Marks the specified document as deleted by adding a field 
        /// _deleted with the value true. 
        /// Documents with this field will not be returned within requests anymore, 
        /// but stay in the database. 
        /// You must supply the current (latest) revision, by using the "_rev" property.
        /// </summary>
        /// <param name="document"><see cref="JObject"/> instance representing a document.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteDocumentAsync(JObject document, bool batch = false)
        {
            var docId = document[IdPropertyName]?.ToString();
            var revision = document[RevisionPropertyName]?.ToString();

            var deletionResponse = await DeleteDocumentAsync(docId, revision, batch);
            document[IdPropertyName] = deletionResponse.Id;
            document[RevisionPropertyName] = deletionResponse.Revision;
        }

        #endregion

        #region Get all docs

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
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
        public abstract Task<DocListResponse<TDocument>> GetAllObjectDocumentsAsync<TDocument>(ListQueryParams queryParams = null, bool extractDocumentAsObject = false, Func<JObject, TDocument> deserializer = null);

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="extractDocumentAsObject">Boolean indicating whether to extract document portion of the 
        /// JSON as object. If False, then the whole JSON is deserialized as object, instead of extracting the 
        /// document portion only.</param>
        /// <returns><see cref="DocListResponse{STRING}"/> containing list of JSON strings.</returns>
        public async Task<DocListResponse<string>> GetAllStringDocumentsAsync(ListQueryParams queryParams = null, bool extractDocumentAsObject = false)
        {
            return await GetAllObjectDocumentsAsync(queryParams, extractDocumentAsObject, deserializer: json => json.ToString());
        }

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="extractDocumentAsObject">Boolean indicating whether to extract document portion of the 
        /// JSON as object. If False, then the whole JSON is deserialized as object, instead of extracting the 
        /// document portion only.</param>
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        public async Task<DocListResponse<JObject>> GetAllJsonDocumentsAsync(ListQueryParams queryParams = null, bool extractDocumentAsObject = false)
        {
            return await GetAllObjectDocumentsAsync(queryParams, extractDocumentAsObject, deserializer: json => json);
        }

        #endregion
    }
}
