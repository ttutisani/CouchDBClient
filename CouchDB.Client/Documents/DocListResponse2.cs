using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents response object for retrieving list of documents.
    /// </summary>
    /// <typeparam name="TDocument">Type of document object.</typeparam>
    public sealed class DocListResponse2<TDocument>
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

        private static List<JObject> JArrayMap(JArray array)
        {
            var result = new List<JObject>();

            for (int index = 0; index < array.Count; index++)
            {
                var currentItem = array[index] as JObject;
                if (currentItem == null)
                    throw new InvalidOperationException($"Cannot retrieve list of objects as item at index '{index}' is null or not '{nameof(JObject)}'.");

                result.Add(currentItem);
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

        internal DocListResponse2(int offset, int totalRows, int updateSeq, IEnumerable<DocListResponseRow<TDocument>> rows)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            Offset = offset;
            TotalRows = totalRows;
            UpdateSeq = updateSeq;

            Rows = new ReadOnlyCollection<DocListResponseRow<TDocument>>(rows.ToList());
        }

        /// <summary>
        /// Offset where the document list started.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Array of view row objects. By default the information returned contains only the document ID and revision.
        /// </summary>
        public ReadOnlyCollection<DocListResponseRow<TDocument>> Rows { get; }

        /// <summary>
        /// Number of documents in the database/view. Note that this is not the number of rows returned in the actual query.
        /// </summary>
        public int TotalRows { get; }

        /// <summary>
        /// Current update sequence for the database.
        /// </summary>
        public int UpdateSeq { get; }

        internal static DocListResponse2<JObject> FromAllDocsJson(JObject allDocsJsonObject)
        {
            Func<JObject, JObject> itemExtractor;
            
            var offset = GetIntOrDefault(allDocsJsonObject, "offset");
            var totalRows = GetIntOrDefault(allDocsJsonObject, "total_rows");
            var updateSeq = GetIntOrDefault(allDocsJsonObject, "update_seq");
            var jsonRows = JArrayMap(GetArrayOrEmpty(allDocsJsonObject, "rows"));
            var responseRows = jsonRows.Select(json => DocListResponseRow<JObject>.FromJson(json));

            return new DocListResponse2<JObject>(offset, totalRows, updateSeq, responseRows);
        }

        internal DocListResponse2<TResult> Cast<TResult>(Func<TDocument, TResult> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            return new DocListResponse2<TResult>(Offset, TotalRows, UpdateSeq, Rows.Select(row => row.Cast(converter)));
        }
    }
}
