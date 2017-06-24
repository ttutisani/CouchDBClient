using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents an abstraction over database for working with documents as entities.
    /// Entities avoid the hassle of manually maintaining ID and Revision for each document.
    /// </summary>
    public sealed class EntityStore
    {
        private readonly ICouchDBDatabase _db;

        /// <summary>
        /// Initializes new instance of <see cref="EntityStore"/> class.
        /// </summary>
        /// <param name="db"></param>
        public EntityStore(ICouchDBDatabase db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _db = db;
        }

        /// <summary>
        /// Saves entity into the database.
        /// </summary>
        /// <param name="entity">Instance of entity to be saved.</param>
        /// <param name="entityUpdateParams">Additional parameters for saving.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveEntityAsync(IEntity entity, DocUpdateParams entityUpdateParams = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var jsonEntity = EntityHelper.ConvertEntityToJSON(entity);

            await _db.SaveJsonDocumentAsync(jsonEntity, entityUpdateParams).Safe();
            entity._id = jsonEntity[CouchDBDatabase.IdPropertyName]?.ToString();
            entity._rev = jsonEntity[CouchDBDatabase.RevisionPropertyName]?.ToString();
        }

        /// <summary>
        /// Retrieves entity from database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be retrieved.</typeparam>
        /// <param name="entityId">ID of entity to be retrieved.</param>
        /// <param name="entityQueryParams">Additional parameters for retrieving.</param>
        /// <returns>Entity.</returns>
        public async Task<TEntity> GetEntityAsync<TEntity>(string entityId, DocQueryParams entityQueryParams = null)
            where TEntity : IEntity
        {
            return await _db.GetObjectDocumentAsync<TEntity>(entityId, entityQueryParams).Safe();
        }

        /// <summary>
        /// Gets all entities from database.
        /// </summary>
        /// <typeparam name="TEntity">Each entity will be casted to this type.</typeparam>
        /// <param name="entityListQueryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{TEntity}"/> containing list of JSON objects (<typeparamref name="TEntity"/>).</returns>
        public async Task<DocListResponse<TEntity>> GetAllEntitiesAsync<TEntity>(ListQueryParams entityListQueryParams = null)
            where TEntity : IEntity
        {
            if (entityListQueryParams == null)
                entityListQueryParams = new ListQueryParams();

            entityListQueryParams.Include_Docs = true;

            return await _db.GetAllObjectDocumentsAsync<TEntity>(entityListQueryParams).Safe();
        }

        /// <summary>
        /// Gets entities from database by list of IDs.
        /// </summary>
        /// <typeparam name="TEntity">Each entity will be casted to this type.</typeparam>
        /// <param name="entityIdList">List if IDs for finding entities.</param>
        /// <param name="entityListQueryParams">Instance of <see cref="ListQueryParams"/> to be used for filtering.</param>
        /// <returns><see cref="DocListResponse{TEntity}"/> containing list of JSON objects (<typeparamref name="TEntity"/>).</returns>
        public async Task<DocListResponse<TEntity>> GetEntitiesAsync<TEntity>(string[] entityIdList, ListQueryParams entityListQueryParams = null)
        {
            if (entityIdList == null)
                throw new ArgumentNullException(nameof(entityIdList));

            if (entityIdList.Length == 0)
                throw new ArgumentException($"{nameof(entityIdList)} should not be empty.", nameof(entityIdList));

            if (entityListQueryParams == null)
                entityListQueryParams = new ListQueryParams();

            entityListQueryParams.Include_Docs = true;

            return await _db.GetObjectDocumentsAsync<TEntity>(entityIdList, entityListQueryParams).Safe();
        }

        /// <summary>
        /// Deletes given entity object.
        /// </summary>
        /// <param name="entity">Entity object to be deleted.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteEntityAsync(IEntity entity, bool batch = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var deletionResponse = await _db.DeleteDocumentAsync(entity._id, entity._rev, batch).Safe();
            entity._id = deletionResponse.Id;
            entity._rev = deletionResponse.Revision;
        }

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
        public async Task<SaveDocListResponse> SaveEntitiesAsync(IEntity[] entities, bool newEdits = true)
        {
            var entityJsonObjects = entities.Select(entity => EntityHelper.ConvertEntityToJSON(entity)).ToArray();

            var saveResponse = await _db.SaveJsonDocumentsAsync(entityJsonObjects, newEdits).Safe();
            if (saveResponse != null)
            {
                for (int index = 0; index < saveResponse.DocumentResponses.Count; index++)
                {
                    if (index >= entities.Length)
                        break;

                    var docResponse = saveResponse.DocumentResponses[index];

                    if (docResponse.Error != null)
                        continue;

                    var entity = entities[index];

                    entity._id = docResponse.Id;
                    entity._rev = docResponse.Revision;
                }
            }

            return saveResponse;
        }

        /// <summary>
        /// Saves attachment on the entity.
        /// </summary>
        /// <param name="entity">Entity to which the attachment will be associated.</param>
        /// <param name="attName">Attachmend name.</param>
        /// <param name="attachment">Attachment content as byte array.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveAttachmentAsync(IEntity entity, string attName, byte[] attachment)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(attName))
                throw new ArgumentNullException(nameof(attName));

            if (attachment == null || attachment.Length == 0)
                throw new ArgumentNullException(nameof(attachment));

            var saveResponse = await _db.SaveAttachmentAsync(entity._id, attName, entity._rev, attachment).Safe();
            entity._rev = saveResponse?.Revision;
        }
    }
}
