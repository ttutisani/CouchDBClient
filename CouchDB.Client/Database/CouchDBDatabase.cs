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
        /// <returns><see cref="DocumentResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<DocumentResponse> SaveDocumentAsync(string docId, string documentJsonString)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(documentJsonString))
                throw new ArgumentNullException(nameof(documentJsonString));

            var newDocResponse = await _http.PutAsync(docId, new StringContent(documentJsonString));
            var dbResponseDTO = await HttpClientHelper.HandleResponse<DocumentResponseDTO>(newDocResponse);

            return new DocumentResponse(dbResponseDTO);
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
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task SaveDocumentAsync(string docId, JObject documentJsonObject)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));

            var saveResponse = await SaveDocumentAsync(docId, documentJsonObject.ToString());
            documentJsonObject[_idPropertyName] = saveResponse.Id;
            documentJsonObject[_revisionPropertyName] = saveResponse.Revision;
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="docId">Document Id.</param>
        /// <param name="documentObject">Document object to be saved.</param>
        /// <returns><see cref="DocumentResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<DocumentResponse> SaveDocumentAsync(string docId, object documentObject)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            return await SaveDocumentAsync(docId, JsonConvert.SerializeObject(documentObject));
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonString">JSON of document to be saved.</param>
        /// <returns><see cref="DocumentResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<DocumentResponse> SaveDocumentAsync(string documentJsonString)
        {
            if (string.IsNullOrWhiteSpace(documentJsonString))
                throw new ArgumentNullException(nameof(documentJsonString));

            var documentJsonObject = JObject.Parse(documentJsonString);
            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            return await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject.ToString());
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentJsonObject">JSON of document to be saved.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task SaveDocumentAsync(JObject documentJsonObject)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));

            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject);
        }

        /// <summary>
        /// Creates a new named document, or creates a new revision of the existing document.
        /// </summary>
        /// <param name="documentObject">Document object to be saved.</param>
        /// <returns><see cref="DocumentResponse"/> with operation results in it.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task<DocumentResponse> SaveDocumentAsync(object documentObject)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            var documentJsonObject = JObject.FromObject(documentObject);
            JToken idPropertyValue;
            documentJsonObject.TryGetValue(_idPropertyName, out idPropertyValue);

            return await SaveDocumentAsync(idPropertyValue?.ToString(), documentJsonObject.ToString());
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
            var documentString = await HttpClientHelper.HandleResponse(docResponse, deserializer: strJson => strJson);

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
    }
}
