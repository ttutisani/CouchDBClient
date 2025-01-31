﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Linq;

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
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task SaveJsonDocumentAsync(this ICouchDBDatabase @this, JObject documentJsonObject, DocUpdateParams updateParams = null)
        {
            if (documentJsonObject == null)
                throw new ArgumentNullException(nameof(documentJsonObject));
            
            var saveResponse = await @this.SaveStringDocumentAsync(documentJsonObject.ToString(), updateParams).Safe();
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
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task<SaveDocResponse> SaveObjectDocumentAsync(this ICouchDBDatabase @this, object documentObject, DocUpdateParams updateParams = null)
        {
            if (documentObject == null)
                throw new ArgumentNullException(nameof(documentObject));

            var documentJsonObject = JObject.FromObject(documentObject);
            if (string.IsNullOrWhiteSpace(documentJsonObject[CouchDBDatabase.IdPropertyName]?.ToString()))
                documentJsonObject.Remove(CouchDBDatabase.IdPropertyName);

            return await @this.SaveStringDocumentAsync(documentJsonObject.ToString(), updateParams).Safe();
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
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task<JObject> GetJsonDocumentAsync(this ICouchDBDatabase @this, string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var jsonString = await @this.GetStringDocumentAsync(docId, queryParams).Safe();
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
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task<TResult> GetObjectDocumentAsync<TResult>(this ICouchDBDatabase @this, string docId, DocQueryParams queryParams = null)
        {
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentNullException(nameof(docId));

            var jsonString = await @this.GetStringDocumentAsync(docId, queryParams).Safe();
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
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="ArgumentException">Document should have _id and _rev.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task DeleteJsonDocumentAsync(this ICouchDBDatabase @this, JObject document, bool batch = false)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var docId = document[CouchDBDatabase.IdPropertyName]?.ToString();
            if (string.IsNullOrWhiteSpace(docId))
                throw new ArgumentException("Document should have _id.", nameof(document));

            var revision = document[CouchDBDatabase.RevisionPropertyName]?.ToString();
            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentException("Document should have _rev.", nameof(document));

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
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public static async Task<DocListResponse<JObject>> GetAllJsonDocumentsAsync(this ICouchDBDatabase @this, ListQueryParams queryParams = null)
        {
            if (queryParams == null)
                queryParams = new ListQueryParams();

            var stringDocs = await @this.GetAllStringDocumentsAsync(queryParams).Safe();

            return stringDocs.Cast(strDoc => !string.IsNullOrWhiteSpace(strDoc) ? JObject.Parse(strDoc) : null);
        }

        /// <summary>
        /// Returns a JSON structure of all of the documents in a given database. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <typeparam name="TDocument">Specifies resulting document object type.</typeparam>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="deserializer">Provide your own deserializer if you prefer. 
        /// By default, it will deserialize by using NewtonSoft.Json methods.
        /// NOTE: if the specified <typeparamref name="TDocument"/> does not have parameterless constructor,
        /// you should specify the deserializer as well. Otherwise, runtime exception will be thrown.</param>
        /// <returns><see cref="DocListResponse{TDOcument}"/> containing list of JSON objects (<typeparamref name="TDocument"/>).</returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public static async Task<DocListResponse<TDocument>> GetAllObjectDocumentsAsync<TDocument>(this ICouchDBDatabase @this, ListQueryParams queryParams = null, Func<string, TDocument> deserializer = null)
        {
            if (queryParams == null)
                queryParams = new ListQueryParams();

            var jsonDocs = await @this.GetAllStringDocumentsAsync(queryParams).Safe();

            return jsonDocs.Cast(deserializer ?? new Func<string, TDocument>(json => !string.IsNullOrWhiteSpace(json) ? JsonConvert.DeserializeObject<TDocument>(json) : default(TDocument)));
        }

        #endregion

        #region Get docs

        /// <summary>
        /// Returns a JSON structure of documents in a given database, by multiple IDs.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="docIdList">Array of document IDs for retrieving documents.</param>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{JObject}"/> containing list of JSON objects (<see cref="JObject"/>).</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public static async Task<DocListResponse<JObject>> GetJsonDocumentsAsync(this ICouchDBDatabase @this, string[] docIdList, ListQueryParams queryParams = null)
        {
            if (docIdList == null || docIdList.Length == 0)
                throw new ArgumentNullException(nameof(docIdList));

            if (queryParams == null)
                queryParams = new ListQueryParams();

            var stringDocs = await @this.GetStringDocumentsAsync(docIdList, queryParams).Safe();

            return stringDocs.Cast(strDoc => !string.IsNullOrWhiteSpace(strDoc) ? JObject.Parse(strDoc) : null);
        }

        /// <summary>
        /// Returns a JSON structure of the documents in a given database, found by ID list. 
        /// The information is returned as a JSON structure containing meta information 
        /// about the return structure, including a list of all documents and basic contents, 
        /// consisting the ID, revision and key. The key is the from the document’s _id.
        /// </summary>
        /// <typeparam name="TDocument">Specifies resulting document object type.</typeparam>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="docIdList">Array of document IDs to be retrieved.</param>
        /// <param name="queryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <param name="deserializer">Provide your own deserializer if you prefer. 
        /// By default, it will deserialize by using NewtonSoft.Json methods.
        /// NOTE: if the specified <typeparamref name="TDocument"/> does not have parameterless constructor,
        /// you should specify the deserializer as well. Otherwise, runtime exception will be thrown.</param>
        /// <returns><see cref="DocListResponse{TDOcument}"/> containing list of JSON objects (<typeparamref name="TDocument"/>).</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        public static async Task<DocListResponse<TDocument>> GetObjectDocumentsAsync<TDocument>(this ICouchDBDatabase @this, string[] docIdList, ListQueryParams queryParams = null, Func<string, TDocument> deserializer = null)
        {
            if (docIdList == null || docIdList.Length == 0)
                throw new ArgumentNullException(nameof(docIdList));

            if (queryParams == null)
                queryParams = new ListQueryParams();

            var stringDocs = await @this.GetStringDocumentsAsync(docIdList, queryParams).Safe();
            return stringDocs.Cast(deserializer ?? new Func<string, TDocument>(strDoc => !string.IsNullOrWhiteSpace(strDoc) ? JsonConvert.DeserializeObject<TDocument>(strDoc) : default(TDocument)));
        }

        #endregion

        #region Save Docs

        /// <summary>
        /// Allows you to create and update multiple documents at the same time within a single request. The basic operation is similar to creating or updating a single document, except that you batch the document structure and information.
        /// When creating new documents the document ID (_id) is optional.
        /// For updating existing documents, you must provide the document ID, revision information (_rev), and new document values.
        /// In case of batch deleting documents all fields as document ID, revision information and deletion status (_deleted) are required.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="documents">List of documents JSON objects.</param>
        /// <param name="newEdits">If false, prevents the database from assigning them new revision IDs. Default is true. Optional</param>
        /// <returns>Instance of <see cref="SaveDocListResponse"/> with detailed information for each requested document to save.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task<SaveDocListResponse> SaveJsonDocumentsAsync(this ICouchDBDatabase @this, JObject[] documents, bool newEdits = true)
        {
            if (documents == null || documents.Length == 0)
                throw new ArgumentNullException(nameof(documents));

            var stringDocs = documents.Select(doc => doc.ToString()).ToArray();
            var saveResponse = await @this.SaveStringDocumentsAsync(stringDocs, newEdits).Safe();
            if (saveResponse != null)
            {
                for (int index = 0; index < saveResponse.DocumentResponses.Count; index++)
                {
                    if (index >= documents.Length)
                        break;

                    var docResponse = saveResponse.DocumentResponses[index];

                    if (docResponse.Error != null)
                        continue;

                    var jsonDoc = documents[index];

                    jsonDoc[CouchDBDatabase.IdPropertyName] = docResponse.Id;
                    jsonDoc[CouchDBDatabase.RevisionPropertyName] = docResponse.Revision;
                }
            }

            return saveResponse;
        }

        /// <summary>
        /// Allows you to create and update multiple documents at the same time within a single request. The basic operation is similar to creating or updating a single document, except that you batch the document structure and information.
        /// When creating new documents the document ID (_id) is optional.
        /// For updating existing documents, you must provide the document ID, revision information (_rev), and new document values.
        /// In case of batch deleting documents all fields as document ID, revision information and deletion status (_deleted) are required.
        /// </summary>
        /// <param name="this">Instance of <see cref="ICouchDBDatabase"/>.</param>
        /// <param name="documents">List of documents objects.</param>
        /// <param name="newEdits">If false, prevents the database from assigning them new revision IDs. Default is true. Optional</param>
        /// <returns>Instance of <see cref="SaveDocListResponse"/> with detailed information for each requested document to save.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public static async Task<SaveDocListResponse> SaveObjectDocumentsAsync(this ICouchDBDatabase @this, object[] documents, bool newEdits = true)
        {
            if (documents == null || documents.Length == 0)
                throw new ArgumentNullException(nameof(documents));

            var stringDocs = documents.Select(doc => JsonConvert.SerializeObject(doc)).ToArray();
            var saveResponse = await @this.SaveStringDocumentsAsync(stringDocs, newEdits).Safe();
            
            return saveResponse;
        }

        #endregion
    }
}
