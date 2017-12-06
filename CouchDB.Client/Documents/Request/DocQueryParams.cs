using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents query string parameters for retrieving document.
    /// </summary>
    public sealed class DocQueryParams : QueryParams
    {
        /// <summary>
        /// Includes attachments bodies in response. Default is false
        /// </summary>
        public bool? Attachments { get; set; }

        /// <summary>
        /// Includes encoding information in attachment stubs if the particular attachment is compressed. 
        /// Default is false.
        /// </summary>
        public bool? Att_Encoding_Info { get; set; }

        /// <summary>
        /// Includes attachments only since specified revisions. 
        /// Doesn’t include attachments for specified revisions. Optional
        /// </summary>
        public IEnumerable<string> Atts_Since { get; set; }

        /// <summary>
        /// Includes information about conflicts in document. Default is false.
        /// </summary>
        public bool? Conflicts { get; set; }

        /// <summary>
        /// Includes information about deleted conflicted revisions. Default is false.
        /// </summary>
        public bool? Deleted_Conflicts { get; set; }

        /// <summary>
        /// Forces retrieving latest “leaf” revision, no matter what rev was requested. 
        /// Default is false.
        /// </summary>
        public bool? Latest { get; set; }

        /// <summary>
        /// Includes last update sequence for the document. Default is false.
        /// </summary>
        public bool? Local_Seq { get; set; }

        /// <summary>
        /// Acts same as specifying all conflicts, deleted_conflicts and open_revs query parameters. 
        /// Default is false.
        /// </summary>
        public bool? Meta { get; set; }

        /// <summary>
        /// Retrieves documents of specified leaf revisions. 
        /// Additionally, it accepts value as all (empty constructor of <see cref="OpenRevs"/>) 
        /// to return all leaf revisions. Optional.
        /// </summary>
        public OpenRevs Open_Revs { get; set; }

        /// <summary>
        /// Retrieves document of specified revision. Optional.
        /// </summary>
        public string Rev { get; set; }

        /// <summary>
        /// Includes list of all known document revisions. Default is false.
        /// </summary>
        public bool? Revs { get; set; }

        /// <summary>
        /// Includes detailed information for all known document revisions. Default is false.
        /// </summary>
        public bool? Revs_Info { get; set; }

        /// <summary>
        /// Converts current instance of <see cref="DocQueryParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal override string ToQueryString()
        {
            var queryParts = new List<string>();

            AddBooleanParam(queryParts, "attachments", Attachments);
            AddBooleanParam(queryParts, "att_encoding_info", Att_Encoding_Info);
            AddStringArrayParam(queryParts, "atts_since", Atts_Since);
            AddBooleanParam(queryParts, "conflicts", Conflicts);
            AddBooleanParam(queryParts, "deleted_conflicts", Deleted_Conflicts);
            AddBooleanParam(queryParts, "latest", Latest);
            AddBooleanParam(queryParts, "local_seq", Local_Seq);
            AddBooleanParam(queryParts, "meta", Meta);
            AddOpenRevsParam(queryParts, "open_revs", Open_Revs);
            AddStringParam(queryParts, "rev", Rev);
            AddBooleanParam(queryParts, "revs", Revs);
            AddBooleanParam(queryParts, "revs_info", Revs_Info);

            return string.Join("&", queryParts);
        }

        private static void AddBooleanParam(List<string> queryParts, string paramName, bool? paramValue)
        {
            if (!paramValue.HasValue)
                return;

            queryParts.Add($"{paramName}={paramValue.Value.ToString().ToLower()}");
        }

        private static void AddStringArrayParam(List<string> queryParts, string paramName, IEnumerable<string> paramValue)
        {
            if (paramValue == null)
                return;

            var paramValuesWithQuotes = paramValue.Select(valueItem => $"\"{valueItem}\"");

            queryParts.Add($"{paramName}=[{string.Join(",", paramValuesWithQuotes)}]");
        }

        private static void AddOpenRevsParam(List<string> queryParts, string paramName, OpenRevs paramValue)
        {
            if (paramValue == null)
                return;

            if (paramValue.Revs == null)
            {
                queryParts.Add($"{paramName}=all");
            }
            else
            {
                AddStringArrayParam(queryParts, paramName, paramValue.Revs);
            }
        }

        private static void AddStringParam(List<string> queryParts, string paramName, string paramValue)
        {
            if (paramValue == null)
                return;

            queryParts.Add($"{paramName}={paramValue}");
        }

        /// <summary>
        /// Represents possible values for 'open_revs' query parameter.
        /// </summary>
        public sealed class OpenRevs
        {
            internal IEnumerable<string> Revs { get; }

            /// <summary>
            /// Denotes all leaf revisions. 
            /// Call this constructor to specify 'all' option for 'open_revs' query parameter.
            /// </summary>
            public OpenRevs()
            {

            }

            /// <summary>
            /// Denotes specific leaf revisions.
            /// Call this constructor to specify revisions array for 'open_revs' query parameter.
            /// </summary>
            /// <param name="revs"></param>
            public OpenRevs(IEnumerable<string> revs)
            {
                Revs = revs;
            }
        }
    }
}
