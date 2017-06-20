using System.Collections.Generic;

namespace CouchDB.Client
{
    internal sealed class AttachmentQueryParams : QueryParams
    {
        public string Rev { get; set; }

        internal override string ToQueryString()
        {
            var queryParts = new List<string>();

            AddStringParam(queryParts, "rev", Rev);

            return string.Join("&", queryParts);
        }

        private static void AddStringParam(List<string> queryParts, string paramName, string paramValue)
        {
            if (paramValue == null)
                return;

            queryParts.Add($"{paramName}={paramValue}");
        }
    }
}
