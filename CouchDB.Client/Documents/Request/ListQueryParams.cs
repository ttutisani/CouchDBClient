﻿using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents query parameters to CouchDB server.
    /// </summary>
    public sealed class ListQueryParams : QueryParams
    {
        /// <summary>
        /// Initializes new instance of <see cref="ListQueryParams"/> type.
        /// </summary>
        public ListQueryParams()
        {
            Include_Docs = true;
        }

        /// <summary>
        /// Includes conflicts information in response. Ignored if include_docs isn’t true. Default is false.
        /// </summary>
        public bool? Conflicts { get; set; }

        /// <summary>
        /// Return the documents in descending by key order. Default is false.
        /// </summary>
        public bool? Descending { get; set; }

        /// <summary>
        /// Stop returning records when the specified key is reached. Optional.
        /// </summary>
        public string EndKey { get; set; }

        /// <summary>
        /// Stop returning records when the specified document ID is reached. Optional.
        /// </summary>
        public string EndKey_DocId { get; set; }

        /// <summary>
        /// Include the full content of the documents in the return. Default is true.
        /// </summary>
        public bool? Include_Docs { get; set; }

        /// <summary>
        /// Specifies whether the specified end key should be included in the result. Default is true.
        /// </summary>
        public bool? Inclusive_End { get; set; }

        /// <summary>
        /// Return only documents that match the specified key. Optional.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Return only documents that match the specified keys. Optional.
        /// </summary>
        public IEnumerable<string> Keys { get; set; }

        /// <summary>
        /// Limit the number of the returned documents to the specified number. Optional.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Skip this number of records before starting to return the results. Default is 0.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Allow the results from a stale view to be used, 
        /// without triggering a rebuild of all views within the encompassing design doc. 
        /// Supported values: ok and update_after. Optional.
        /// </summary>
        public StaleOption? Stale { get; set; }

        /// <summary>
        /// Return records starting with the specified key. Optional.
        /// </summary>
        public string StartKey { get; set; }

        /// <summary>
        /// Return records starting with the specified document ID. Optional.
        /// </summary>
        public string StartKey_DocId { get; set; }

        /// <summary>
        /// Response includes an update_seq value indicating which sequence id of the underlying database 
        /// the view reflects. Default is false.
        /// </summary>
        public bool? Update_Seq { get; set; }

        /// <summary>
        /// Converts current instance of <see cref="ListQueryParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal override string ToQueryString()
        {
            var queryParts = new List<string>();

            AddBooleanParam(queryParts, "conflicts", Conflicts);
            AddBooleanParam(queryParts, "descending", Descending);
            AddStringParam(queryParts, "endkey", EndKey);
            AddStringParam(queryParts, "endkey_docid", EndKey_DocId);
            AddBooleanParam(queryParts, "include_docs", Include_Docs);
            AddBooleanParam(queryParts, "inclusive_end", Inclusive_End);
            AddStringParam(queryParts, "key", Key);
            AddStringArrayParam(queryParts, "keys", Keys);
            AddNumberParam(queryParts, "limit", Limit);
            AddNumberParam(queryParts, "skip", Skip);
            AddEnumParam(queryParts, "stale", Stale);
            AddStringParam(queryParts, "startkey", StartKey);
            AddStringParam(queryParts, "startkey_docid", StartKey_DocId);
            AddBooleanParam(queryParts, "update_seq", Update_Seq);

            return string.Join("&", queryParts);
        }

        private static void AddBooleanParam(List<string> queryParts, string paramName, bool? paramValue)
        {
            if (!paramValue.HasValue)
                return;

            queryParts.Add($"{paramName}={paramValue.Value}");
        }

        private static void AddStringParam(List<string> queryParts, string paramName, string paramValue)
        {
            if (paramValue == null)
                return;

            queryParts.Add($"{paramName}=\"{paramValue}\"");
        }

        private static void AddStringArrayParam(List<string> queryParts, string paramName, IEnumerable<string> paramValue)
        {
            if (paramValue == null)
                return;

            var paramValuesWithQuotes = paramValue.Select(valueItem => $"\"{valueItem}\"");

            queryParts.Add($"{paramName}=[{string.Join(",", paramValuesWithQuotes)}]");
        }

        private static void AddNumberParam(List<string> queryParts, string paramName, int? paramValue)
        {
            if (!paramValue.HasValue)
                return;

            queryParts.Add($"{paramName}={paramValue.Value}");
        }

        private static void AddEnumParam<TEnum>(List<string> queryParts, string paramName, TEnum? paramValue)
            where TEnum: struct
        {
            if (!paramValue.HasValue)
                return;

            queryParts.Add($"{paramName}={paramValue}");
        }

        /// <summary>
        /// Enumeration of options for stale data.
        /// </summary>
        public enum StaleOption
        {
            /// <summary>
            /// Ok to include stale data.
            /// </summary>
            Ok,

            /// <summary>
            /// Ok to include, but update after fetch.
            /// </summary>
            Update_After
        }

        internal static ListQueryParams CreateEmpty()
        {
            return new ListQueryParams { Include_Docs = null };
        }
    }
}
