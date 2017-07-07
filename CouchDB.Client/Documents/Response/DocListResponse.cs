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
    public sealed class DocListResponse<TDocument>
    {
        internal DocListResponse(int offset, int totalRows, int updateSeq, IEnumerable<DocListResponseRow<TDocument>> rows)
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

        internal static DocListResponse<string> FromJsonToStringList(string allDocsJsonString)
        {
            var allDocsJsonObject = JObject.Parse(allDocsJsonString);

            var offset = SerializationHelper.GetIntOrDefault(allDocsJsonObject, "offset");
            var totalRows = SerializationHelper.GetIntOrDefault(allDocsJsonObject, "total_rows");
            var updateSeq = SerializationHelper.GetIntOrDefault(allDocsJsonObject, "update_seq");
            var jsonRows = SerializationHelper.JArrayMap(SerializationHelper.GetArrayOrEmpty(allDocsJsonObject, "rows"));
            var responseRows = jsonRows.Select(json => DocListResponseRow<string>.FromJsonToString(json)).ToList();

            return new DocListResponse<string>(offset, totalRows, updateSeq, responseRows);
        }
        
        internal DocListResponse<TResult> Cast<TResult>(Func<TDocument, TResult> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            return new DocListResponse<TResult>(Offset, TotalRows, UpdateSeq, Rows.Select(row => row.Cast(converter)));
        }
    }
}
