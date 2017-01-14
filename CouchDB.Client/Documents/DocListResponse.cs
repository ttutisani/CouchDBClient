using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static List<JObject> JArrayMap(JArray array, Func<JObject, JObject> itrmExtractor)
        {
            var result = new List<JObject>();

            for (int index = 0; index < array.Count; index++)
            {
                var currentItem = array[index] as JObject;
                if (currentItem == null)
                    throw new InvalidOperationException($"Cannot retrieve list of objects as item at index '{index}' is null or not '{nameof(JObject)}'.");

                var convertedItem = itrmExtractor(currentItem);
                
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

        internal DocListResponse(int offset, int totalRows, int updateSeq, IEnumerable<TDocument> rows)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            Offset = offset;
            TotalRows = totalRows;
            UpdateSeq = updateSeq;

            Rows = rows.ToArray();
        }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Array of view row objects. By default the information returned contains only the document ID and revision.
        /// </summary>
        public TDocument[] Rows { get; }

        /// <summary>
        /// Number of documents in the database/view. Note that this is not the number of rows returned in the actual query.
        /// </summary>
        public int TotalRows { get; }

        /// <summary>
        /// Current update sequence for the database.
        /// </summary>
        public int UpdateSeq { get; }

        internal static DocListResponse<JObject> FromAllDocsJson(JObject allDocsJsonObject, bool extractDocumentAsObject = false)
        {
            Func<JObject, JObject> itemExtractor;
            if (extractDocumentAsObject)
                itemExtractor = jObject => GetObjectOrThrow(jObject, "doc");
            else
                itemExtractor = jObject => jObject;

            var offset = GetIntOrDefault(allDocsJsonObject, "offset");
            var totalRows = GetIntOrDefault(allDocsJsonObject, "total_rows");
            var updateSeq = GetIntOrDefault(allDocsJsonObject, "update_seq");
            var rows = JArrayMap(GetArrayOrEmpty(allDocsJsonObject, "rows"), itemExtractor);

            return new DocListResponse<JObject>(offset, totalRows, updateSeq, rows);
        }

        internal DocListResponse<TResult> Cast<TResult>(Func<TDocument, TResult> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            return new DocListResponse<TResult>(Offset, TotalRows, UpdateSeq, Rows.Select(doc => converter(doc)));
        }
    }
}
