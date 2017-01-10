using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
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

        internal sealed class SaveDocResponseDTO
        {
            public string Id { get; set; }

            public string Rev { get; set; }
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
            var newDocResponse = await _http.PostAsync(newDocUrl, new StringContent(documentJsonString, Encoding.UTF8, "application/json"));
            var docResponseDTO = await HttpClientHelper.HandleResponse<SaveDocResponseDTO>(newDocResponse, false);

            return new SaveDocResponse(docResponseDTO);
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

            var saveResponse = await SaveDocumentAsync(documentJsonObject.ToString(), updateParams);
            documentJsonObject[_idPropertyName] = saveResponse.Id;
            documentJsonObject[_revisionPropertyName] = saveResponse.Revision;
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
        public async Task<string> GetDocumentAsync(string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var docQuery = QueryParams.AppendQueryParams(docId, queryParams);

            var docResponse = await _http.GetAsync(docQuery);
            var documentString = await HttpClientHelper.HandleStringResponse(docResponse, true);

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

            var deleteResponse = await _http.DeleteAsync(deleteDocUrl);
            var saveDTO = await HttpClientHelper.HandleResponse<SaveDocResponseDTO>(deleteResponse, convertNotFoundIntoNull: true)
                ?? new SaveDocResponseDTO { Id = docId, Rev = revision };

            return new SaveDocResponse(saveDTO);
        }

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
            var docId = document[_idPropertyName]?.ToString();
            var revision = document[_revisionPropertyName]?.ToString();

            var deletionResponse = await DeleteDocumentAsync(docId, revision, batch);
            document[_idPropertyName] = deletionResponse.Id;
            document[_revisionPropertyName] = deletionResponse.Revision;
        }

        #endregion

        #region Get all docs

        private async Task<JObject> GetAllDocumentsObjectAsync(ListQueryParams queryParams)
        {
            var allDocsUrl = QueryParams.AppendQueryParams("_all_docs", queryParams);

            var allDocsResponse = await _http.GetAsync(allDocsUrl);
            var allDocsJsonString = await HttpClientHelper.HandleStringResponse(allDocsResponse, false);
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
        /// <returns><see cref="DocListResponse{STRING}"/> containing list of JSON strings.</returns>
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
        /// <exception cref="ArgumentException"><paramref name="extractDocumentAsObject"/> can be true only when Include_Docs is true within <paramref name="queryParams"/>.</exception>
        public async Task<DocListResponse<TDocument>> GetAllObjectDocumentsAsync<TDocument>(ListQueryParams queryParams = null, bool extractDocumentAsObject = false, Func<JObject, TDocument> deserializer = null)
        {
            if (extractDocumentAsObject && (queryParams?.Include_Docs != true))
                throw new ArgumentException($"'{nameof(extractDocumentAsObject)}' can be {true} only when '{nameof(queryParams.Include_Docs)}' is {true} within {nameof(queryParams)}.");

            var allDocsJsonObject = await GetAllDocumentsObjectAsync(queryParams);

            var docListResponse = DocListResponse<TDocument>.FromCustomObjects(allDocsJsonObject, extractDocumentAsObject, deserializer);
            return docListResponse;
        }

        #endregion

        #region Convenience entity operations

        /// <summary>
        /// Saves entity into the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be saved.</typeparam>
        /// <param name="entity">Instance of entity to be saved.</param>
        /// <param name="entityUpdateParams">Additional parameters for saving.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveEntityAsync<TEntity>(TEntity entity, DocUpdateParams entityUpdateParams = null)
            where TEntity: IEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var jsonEntity = JObject.FromObject(entity);
            if (string.IsNullOrWhiteSpace(entity._rev))
            {
                jsonEntity.Remove(_revisionPropertyName);
            }

            await SaveDocumentAsync(jsonEntity, entityUpdateParams);
            entity._id = jsonEntity[_idPropertyName]?.ToString();
            entity._rev = jsonEntity[_revisionPropertyName]?.ToString();
        }

        /// <summary>
        /// Retrieves entity from database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be retrieved.</typeparam>
        /// <param name="entityId">ID of entity to be retrieved.</param>
        /// <param name="entityQueryParams">Additional parameters for retrieving.</param>
        /// <returns>Entity.</returns>
        public async Task<TEntity> GetEntityAsync<TEntity>(string entityId, DocQueryParams entityQueryParams = null)
            where TEntity: IEntity
        {
            return await GetDocumentAsync<TEntity>(entityId);
        }

        /// <summary>
        /// Gets all entities from database.
        /// </summary>
        /// <typeparam name="TEntity">Each entity will be casted to this type.</typeparam>
        /// <param name="entityListQueryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{TEntity}"/> containing list of JSON objects (<typeparamref name="TEntity"/>).</returns>
        public async Task<DocListResponse<TEntity>> GetAllEntitiesAsync<TEntity>(ListQueryParams entityListQueryParams = null)
            where TEntity: IEntity
        {
            if (entityListQueryParams == null)
                entityListQueryParams = new ListQueryParams();

            entityListQueryParams.Include_Docs = true;

            return await GetAllObjectDocumentsAsync<TEntity>(entityListQueryParams, extractDocumentAsObject: true);
        }

        /// <summary>
        /// Deletes given entity object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be deleted.</typeparam>
        /// <param name="entity">Entity object to be deleted.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteEntityAsync<TEntity>(TEntity entity, bool batch = false)
            where TEntity: IEntity
        {
            var deletionResponse = await DeleteDocumentAsync(entity._id, entity._rev, batch);
            entity._id = deletionResponse.Id;
            entity._rev = deletionResponse.Revision;
        }

        #endregion
    }
}
