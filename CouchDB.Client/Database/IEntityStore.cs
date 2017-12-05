using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents an abstraction over database for working with documents as entities.
    /// Entities avoid the hassle of manually maintaining ID and Revision for each document.
    /// </summary>
    public interface IEntityStore
    {
        /// <summary>
        /// Deletes attachment from database.
        /// </summary>
        /// <param name="entity">Instance of entity owning the attachment to delete.</param>
        /// <param name="attName">Attachment name.</param>
        /// <param name="batch">Store changes in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task DeleteAttachmentAsync(IEntity entity, string attName, bool batch = false);

        /// <summary>
        /// Deletes given entity object.
        /// </summary>
        /// <param name="entity">Entity object to be deleted.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task DeleteEntityAsync(IEntity entity, bool batch = false);

        /// <summary>
        /// Gets all entities from database.
        /// </summary>
        /// <typeparam name="TEntity">Each entity will be casted to this type.</typeparam>
        /// <param name="entityListQueryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{TEntity}"/> containing list of JSON objects (<typeparamref name="TEntity"/>).</returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        Task<DocListResponse<TEntity>> GetAllEntitiesAsync<TEntity>(ListQueryParams entityListQueryParams = null) where TEntity : IEntity;

        /// <summary>
        /// Retrieves attachment from database.
        /// </summary>
        /// <param name="entity">Entity instance which owns the attachment.</param>
        /// <param name="attName">Attachment name.</param>
        /// <returns>Attachment as byte array.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<byte[]> GetAttachmentAsync(IEntity entity, string attName);

        /// <summary>
        /// Gets entities from database by list of IDs.
        /// </summary>
        /// <typeparam name="TEntity">Each entity will be casted to this type.</typeparam>
        /// <param name="entityIdList">List if IDs for finding entities.</param>
        /// <param name="entityListQueryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{TEntity}"/> containing list of JSON objects (<typeparamref name="TEntity"/>).</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        /// <exception cref="InvalidOperationException">Malformed JSON string received from CouchDB server..</exception>
        Task<DocListResponse<TEntity>> GetEntitiesAsync<TEntity>(string[] entityIdList, ListQueryParams entityListQueryParams = null);

        /// <summary>
        /// Retrieves entity from database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be retrieved.</typeparam>
        /// <param name="entityId">ID of entity to be retrieved.</param>
        /// <param name="entityQueryParams">Additional parameters for retrieving.</param>
        /// <returns>Entity.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<TEntity> GetEntityAsync<TEntity>(string entityId, DocQueryParams entityQueryParams = null) where TEntity : IEntity;

        /// <summary>
        /// Saves attachment on the entity.
        /// </summary>
        /// <param name="entity">Entity to which the attachment will be associated.</param>
        /// <param name="attName">Attachmend name.</param>
        /// <param name="attachment">Attachment content as byte array.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task SaveAttachmentAsync(IEntity entity, string attName, byte[] attachment);

        /// <summary>
        /// Allows you to create and update multiple entities at the same time within a single request. The basic operation is similar to creating or updating a single document, except that you batch the document structure and information.
        /// When creating new documents the document ID (_id) is optional.
        /// For updating existing documents, you must provide the document ID, revision information (_rev), and new document values.
        /// In case of batch deleting documents all fields as document ID, revision information and deletion status (_deleted) are required.
        /// </summary>
        /// <param name="entities">List of documents objects.</param>
        /// <param name="newEdits">If false, prevents the database from assigning them new revision IDs. Default is true. Optional</param>
        /// <returns>Instance of <see cref="SaveDocListResponse"/> with detailed information for each requested document to save.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task<SaveDocListResponse> SaveEntitiesAsync(IEntity[] entities, bool newEdits = true);

        /// <summary>
        /// Saves entity into the database.
        /// </summary>
        /// <param name="entity">Instance of entity to be saved.</param>
        /// <param name="entityUpdateParams">Additional parameters for saving.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        Task SaveEntityAsync(IEntity entity, DocUpdateParams entityUpdateParams = null);
    }
}