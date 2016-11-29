using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents signle database within CouchDB server instance.
    /// </summary>
    public sealed class CouchDBDatabase : IDisposable
    {
        private const string _idPropertyName = "_id";
        private const string _revisionPropertyName = "_rev";

        private readonly HttpClient _http;

        internal CouchDBDatabase(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            Uri dbUri;
            if (!Uri.TryCreate(UrlHelper.CombineUrl(baseUrl, "/"), UriKind.Absolute, out dbUri))
                throw new FormatException("URL is not in valid format.");

            _http = new HttpClient();
            _http.BaseAddress = dbUri;
        }

        /// <summary>
        /// Disposes current instance of <see cref="CouchDBDatabase"/>, after which it becomes unusable.
        /// </summary>
        public void Dispose()
        {
            _http.Dispose();
        }

        #region Save doc

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="docId">Document Id.</param>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<SaveDocResponse> SaveDocumentAsync(string docId, string documentJsonString, DocUpdateParams updateParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(documentJsonString))
                throw new ArgumentNullException(nameof(documentJsonString));

            var newDocUrl = QueryParams.AppendQueryParams(docId, updateParams);

            var newDocResponse = await _http.PutAsync(docId, new StringContent(documentJsonString));
            var dbResponseDTO = await HttpClientHelper.HandleResponse<DocumentResponseDTO>(newDocResponse);

            return new SaveDocResponse(dbResponseDTO);
        }

        internal sealed class DocumentResponseDTO
        {
            public string Id { get; set; }

            public string Rev { get; set; }
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="docId">Document Id.</param>
        /// <param name="documentJsonObject">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task SaveDocumentAsync(string docId, JObject documentJsonObject, DocUpdateParams updateParams = null)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));

            var saveResponse = await SaveDocumentAsync(docId, documentJsonObject.ToString(), updateParams);
            documentJsonObject[_idPropertyName] = saveResponse.Id;
            documentJsonObject[_revisionPropertyName] = saveResponse.Revision;
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="docId">Document Id.</param>
        /// <param name="documentObject">Document object to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<SaveDocResponse> SaveDocumentAsync(string docId, object documentObject, DocUpdateParams updateParams = null)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            return await SaveDocumentAsync(docId, JsonConvert.SerializeObject(documentObject), updateParams);
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <param name="updateParams">Query parameters for updating document.</param>
        /// <returns><see cref="SaveDocResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<SaveDocResponse> SaveDocumentAsync(string documentJsonString, DocUpdateParams updateParams = null)
        {
            if (string.IsNullOrWhiteSpace(documentJsonString))
                throw new ArgumentNullException(nameof(documentJsonString));

            var documentJsonObject = JObject.Parse(documentJsonString);
            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            return await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject.ToString(), updateParams);
        }

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

            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject, updateParams);
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

            var documentJsonObject = JObject.FromObject(documentObject);
            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            return await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject.ToString(), updateParams);
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
        public async Task<string> GetDocumentAsync(string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var docQuery = QueryParams.AppendQueryParams(docId, queryParams);

            var docResponse = await _http.GetAsync(docQuery);
            var documentString = await HttpClientHelper.HandleStringResponse(docResponse);

            return documentString;
        }

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
            var jsonObject = JObject.Parse(jsonString);

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
            var resultObject = JsonConvert.DeserializeObject<TResult>(jsonString);

            return resultObject;
        }

        #endregion

        #region Get all docs

        private async Task<JObject> GetAllDocumentsObjectAsync(ListQueryParams queryParams)
        {
            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);

            var allDocsResponse = await _http.GetAsync(allDocsUrl);
            var allDocsJsonString = await HttpClientHelper.HandleStringResponse(allDocsResponse);
            var allDocsJsonObject = JObject.Parse(allDocsJsonString);

            return allDocsJsonObject;
        }

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{string}"/> containing list of JSON strings.</returns>
        public async Task<DocListResponse<string>> GetAllStringDocumentsAsync(ListQueryParams queryParams = null)
        {
            var allDocsJsonObject = await GetAllDocumentsObjectAsync(queryParams);

            var docListResponse = DocListResponse<string>.FromJsonStrings(allDocsJsonObject);
            return docListResponse;
        }

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        public async Task<DocListResponse<JObject>> GetAllJsonDocumentsAsync(ListQueryParams queryParams = null)
        {
            var allDocsJsonObject = await GetAllDocumentsObjectAsync(queryParams);

            var docListResponse = DocListResponse<JObject>.FromJsonObjects(allDocsJsonObject);
            return docListResponse;
        }

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
        public async Task<DocListResponse<TDocument>> GetAllObjectDocumentsAsync<TDocument>(ListQueryParams queryParams = null, bool extractDocumentAsObject = false, Func<JObject, TDocument> deserializer = null)
        {
            if (extractDocumentAsObject && (queryParams?.Include_Docs != true))
                throw new ArgumentException($"'{nameof(extractDocumentAsObject)}' can be {true} only when '{nameof(queryParams.Include_Docs)}' is {true} within {nameof(queryParams)}.");

            var allDocsJsonObject = await GetAllDocumentsObjectAsync(queryParams);

            var docListResponse = DocListResponse<TDocument>.FromCustomObjects(allDocsJsonObject, extractDocumentAsObject, deserializer);
            return docListResponse;
        }

        #endregion
    }
}
