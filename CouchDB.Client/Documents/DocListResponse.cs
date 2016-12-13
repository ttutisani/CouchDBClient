using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents response object for retrieving list of documents.
    /// </summary>
    /// <typeparam name="TDocument">Type of document object.</typeparam>
    public sealed class DocListResponse<TDocument>
    {
        private static int GetIntOrDefault(JObject source, string propertyName)
        {
            JToken propertyToken;
            if (source.TryGetValue(propertyName, out propertyToken) && propertyToken.Type == JTokenType.Integer)
            {
                return propertyToken.Value<int>();
            }

            return 0;
        }

        private static JArray GetArrayOrEmpty(JObject source, string propertyName)
        {
            var arrayValue = source[propertyName] as JArray;
            return arrayValue ?? new JArray();
        }

        private static List<TDocument> JArrayMap(JArray array, Func<JObject, TDocument> itemConverter)
        {
            var result = new List<TDocument>();

            for (int index = 0; index < array.Count; index++)
            {
                var currentItem = array[index] as JObject;
                if (currentItem == null)
                    throw new InvalidOperationException($"Cannot retrieve list of objects as item at index '{index}' is null or not '{nameof(JObject)}'.");

                var convertedItem = itemConverter(currentItem);
                
                result.Add(convertedItem);
            }

            return result;
        }

        private static JObject GetObjectOrThrow(JObject source, string propertyName)
        {
            var propValue = source[propertyName] as JObject;
            if (propValue == null)
                throw new InvalidOperationException($"Given {nameof(JObject)} does not have property '{nameof(propertyName)}' or it's not {nameof(JObject)}.");

            return propValue;
        }

        private DocListResponse(JObject allDocsJsonObject, Func<JObject, TDocument> itemConverter)
        {
            if (allDocsJsonObject == null)
                throw new ArgumentNullException(nameof(allDocsJsonObject));

            Offset = GetIntOrDefault(allDocsJsonObject, "offset");
            TotalRows = GetIntOrDefault(allDocsJsonObject, "total_rows");
            UpdateSeq = GetIntOrDefault(allDocsJsonObject, "update_seq");

            var rowsArray = GetArrayOrEmpty(allDocsJsonObject, "rows");
            Rows.AddRange(JArrayMap(rowsArray, itemConverter));
        }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Array of view row objects. By default the information returned contains only the document ID and revision.
        /// </summary>
        public List<TDocument> Rows { get; } = new List<TDocument>();

        /// <summary>
        /// Number of documents in the database/view. Note that this is not the number of rows returned in the actual query.
        /// </summary>
        public int TotalRows { get; }

        /// <summary>
        /// Current update sequence for the database.
        /// </summary>
        public int UpdateSeq { get; }

        internal static DocListResponse<JObject> FromJsonObjects(JObject allDocsJsonObject)
        {
            return new DocListResponse<JObject>(allDocsJsonObject, obj => obj);
        }

        internal static DocListResponse<string> FromJsonStrings(JObject allDocsJsonObject)
        {
            return new DocListResponse<string>(allDocsJsonObject, obj => obj.ToString());
        }

        internal static DocListResponse<TDocument> FromCustomObjects(JObject allDocsJsonObject, bool extractDocumentAsObject = false, Func<JObject, TDocument> deserializer = null)
        {
            if (deserializer == null)
                deserializer = jObject => jObject.ToObject<TDocument>();

            Func<JObject, TDocument> itemConverter;
            if (extractDocumentAsObject)
                itemConverter = jObject => deserializer(GetObjectOrThrow(jObject, "doc"));
            else
                itemConverter = jObject => deserializer(jObject);

            return new DocListResponse<TDocument>(allDocsJsonObject, itemConverter);
        }
    }
}
