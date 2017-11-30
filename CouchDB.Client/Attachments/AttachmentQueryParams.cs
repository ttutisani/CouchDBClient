using System.Collections.Generic;

namespace CouchDB.Client
{
    internal sealed class AttachmentQueryParams : QueryParams
    {
        public string Rev { get; set; }

        /// <summary>
        /// Converts current instance of <see cref="AttachmentQueryParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NoException"></exception> //TODO: remove all KeyNotFoundException comments (this is just a marker for NONE).
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
