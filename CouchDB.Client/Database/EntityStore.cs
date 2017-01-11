﻿using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents an abstraction over database for working with documents as entities.
    /// Entities avoid the hassle of manually maintaining ID and Revision for each document.
    /// </summary>
    public sealed class EntityStore
    {
        private readonly CouchDBDatabase _db;

        /// <summary>
        /// Initializes new instance of <see cref="EntityStore"/> class.
        /// </summary>
        /// <param name="db"></param>
        public EntityStore(CouchDBDatabase db)
        {
            if (db == null)
                throw new ArgumentNullException(nameof(db));

            _db = db;
        }

        /// <summary>
        /// Saves entity into the database.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be saved.</typeparam>
        /// <param name="entity">Instance of entity to be saved.</param>
        /// <param name="entityUpdateParams">Additional parameters for saving.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SaveEntityAsync<TEntity>(TEntity entity, DocUpdateParams entityUpdateParams = null)
            where TEntity : IEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var jsonEntity = JObject.FromObject(entity);
            if (string.IsNullOrWhiteSpace(entity._rev))
            {
                jsonEntity.Remove(CouchDBDatabase.RevisionPropertyName);
            }

            await _db.SaveDocumentAsync(jsonEntity, entityUpdateParams);
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
            return await _db.GetDocumentAsync<TEntity>(entityId);
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

            return await _db.GetAllObjectDocumentsAsync<TEntity>(entityListQueryParams, extractDocumentAsObject: true);
        }

        /// <summary>
        /// Deletes given entity object.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to be deleted.</typeparam>
        /// <param name="entity">Entity object to be deleted.</param>
        /// <param name="batch">Stores document in batch mode Possible values: ok (when set to true). Optional.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteEntityAsync<TEntity>(TEntity entity, bool batch = false)
            where TEntity : IEntity
        {
            var deletionResponse = await _db.DeleteDocumentAsync(entity._id, entity._rev, batch);
            entity._id = deletionResponse.Id;
            entity._rev = deletionResponse.Revision;
        }
    }
}