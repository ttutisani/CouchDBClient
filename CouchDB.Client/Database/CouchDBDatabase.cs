using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        internal sealed class SaveDocResponseDTO
        {
            public string Id { get; set; }

            public string Rev { get; set; }

            public string Error { get; set; }

            public string Reason { get; set; }
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
            
            var newDocUrl = QueryParams.AppendQueryParams(string.Empty, updateParams);
            var newDocResponse = await _http.PostAsync(newDocUrl, new StringContent(documentJsonString, Encoding.UTF8, "application/json")).Safe();
            var docResponseDTO = await HttpClientHelper.HandleResponse<SaveDocResponseDTO>(newDocResponse, false).Safe();

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
        public async Task<string> GetDocumentAsync(string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var docQuery = QueryParams.AppendQueryParams(docId, queryParams);

            var docResponse = await _http.GetAsync(docQuery).Safe();
            var documentString = await HttpClientHelper.HandleStringResponse(docResponse, true).Safe();

            return documentString;
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
        public async Task<SaveDocResponse> DeleteDocumentAsync(string docId, string revision, bool batch = false)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentNullException(nameof(revision));

            var deleteQueryParams = new DeleteDocParams
            {
                Revision = revision,
                Batch = batch
            };
            var deleteDocUrl = QueryParams.AppendQueryParams(docId, deleteQueryParams);

            var deleteResponse = await _http.DeleteAsync(deleteDocUrl).Safe();
            var saveDTO = await HttpClientHelper.HandleResponse<SaveDocResponseDTO>(deleteResponse, convertNotFoundIntoNull: true).Safe()
                ?? new SaveDocResponseDTO { Id = docId, Rev = revision };

            return new SaveDocResponse(saveDTO);
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
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        public async Task<DocListResponse<JObject>> GetAllJsonDocumentsAsync(ListQueryParams queryParams = null)
        {
            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);

            var allDocsResponse = await _http.GetAsync(allDocsUrl).Safe();
            var allDocsJsonString = await HttpClientHelper.HandleStringResponse(allDocsResponse, false).Safe();
            var allDocsJsonObject = JObject.Parse(allDocsJsonString);

            var docListResponse = DocListResponse<JObject>.FromJson(allDocsJsonObject);
            return docListResponse;
        }

        #endregion

        #region Get Docs

        /// <summary>
        /// Returns a JSON structure of documents in a given database, by multiple IDs.
        /// </summary>
        /// <param name="docIdList">Array of document IDs for retrieving documents.</param>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        public async Task<DocListResponse<JObject>> GetJsonDocumentsAsync(string[] docIdList, ListQueryParams queryParams = null)
        {
            if (docIdList == null)
                throw new ArgumentNullException(nameof(docIdList));

            if (docIdList.Length == 0)
                throw new ArgumentException($"{nameof(docIdList)} should not be empty.", nameof(docIdList));

            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);
            var allDocsRequest = new { keys = docIdList };
            var allDocsJsonRequest = JsonConvert.SerializeObject(allDocsRequest);

            var allDocsResponse = await _http.PostAsync(allDocsUrl, new StringContent(allDocsJsonRequest, Encoding.UTF8, "application/json")).Safe();
            var allDocsJsonString = await HttpClientHelper.HandleStringResponse(allDocsResponse, false).Safe();
            var allDocsJsonObject = JObject.Parse(allDocsJsonString);

            var docListResponse = DocListResponse<JObject>.FromJson(allDocsJsonObject);
            return docListResponse;
        }

        #endregion

        #region Save Docs

        internal sealed class SaveDocListResponseDTO : List<SaveDocResponseDTO>
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
        public async Task<SaveDocListResponse> SaveDocumentsAsync(string[] documents, bool newEdits = true)
        {
            if (documents == null || documents.Length == 0)
                throw new ArgumentNullException(nameof(documents));

            var saveDocListRequest = new SaveDocListRequest(newEdits);
            saveDocListRequest.AddDocuments(documents);
            var saveDocListRequestJson = saveDocListRequest.ToJson().ToString();

            var saveDocListResponse = await _http.PostAsync("_bulk_docs", new StringContent(saveDocListRequestJson, Encoding.UTF8, "application/json")).Safe();
            var saveDocListResponseDTO = await HttpClientHelper.HandleResponse<SaveDocListResponseDTO>(saveDocListResponse, false).Safe();

            return new SaveDocListResponse(saveDocListResponseDTO);
        }

        #endregion
    }
}
