﻿using Newtonsoft.Json.Linq;

namespace CouchDB.Client
{
    internal static class EntityHelper
    {
        /// <summary>
        /// Convert entity to JSON (honoring Id and Revision).
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        public static JObject ConvertEntityToJSON(IEntity entity)
        {
            if (entity == null)
                return null;

            var json = JObject.FromObject(entity);
            if (string.IsNullOrWhiteSpace(entity._id))
                json.Remove(CouchDBDatabase.IdPropertyName);
            if (string.IsNullOrWhiteSpace(entity._rev))
                json.Remove(CouchDBDatabase.RevisionPropertyName);

            return json;
        }
    }
}
