using System.Collections.Generic;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents query parameters for updating CouchDB document.
    /// </summary>
    public sealed class DocUpdateParams : QueryParams
    {
        /// <summary>
        /// Stores document in batch mode. Possible values: ok (when assigned true). Optional.
        /// </summary>
        public bool Batch { get; set; }

        /// <summary>
        /// Prevents insertion of a conflicting document. Possible values: true (default) and false. 
        /// If false, a well-formed _rev must be included in the document. 
        /// new_edits=false is used by the replicator to insert documents into the target database 
        /// even if that leads to the creation of conflicts. Optional.
        /// </summary>
        public bool? New_Edits { get; set; }

        /// <summary>
        /// Converts current instance of <see cref="DocUpdateParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal override string ToQueryString()
        {
            var queryParts = new List<string>();

            AddBatchParam(queryParts, "batch", Batch);
            AddBooleanParam(queryParts, "new_edits", New_Edits);

            return string.Join("&", queryParts);
        }

        private static void AddBatchParam(List<string> queryParts, string paramName, bool batch)
        {
            if (!batch)
                return;

            queryParts.Add($"{paramName}=ok");
        }

        private static void AddBooleanParam(List<string> queryParts, string paramName, bool? paramValue)
        {
            if (!paramValue.HasValue)
                return;

            queryParts.Add($"{paramName}={paramValue.Value.ToString().ToLower()}");
        }
    }
}
