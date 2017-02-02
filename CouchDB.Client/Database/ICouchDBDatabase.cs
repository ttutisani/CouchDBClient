using Newtonsoft.Json.Linq;
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
        Task<SaveDocResponse> SaveDocumentAsync(string documentJsonString, DocUpdateParams updateParams = null);

        /// <summary>
        /// Returns document by the specified docid from the specified db. 
        /// Unless you request a specific revision, the latest revision of the document will always be returned.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="queryParams">Additional query parameters for retrieving document.</param>
        /// <returns><see cref="string"/> containing document JSON.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        Task<string> GetDocumentAsync(string docId, DocQueryParams queryParams = null);

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
        Task<SaveDocResponse> DeleteDocumentAsync(string docId, string revision, bool batch = false);

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
        Task<DocListResponse2<JObject>> GetAllJsonDocumentsAsync(ListQueryParams queryParams = null);
    }
}
