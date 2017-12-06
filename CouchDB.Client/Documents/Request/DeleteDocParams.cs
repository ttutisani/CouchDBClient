using System.Collections.Generic;

namespace CouchDB.Client
{
    internal sealed class DeleteDocParams : QueryParams
    {
        public string Revision { get; set; }

        public bool Batch { get; set; }

        /// <summary>
        /// Converts current instance of <see cref="DeleteDocParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal override string ToQueryString()
        {
            var queryParts = new List<string>();

            AddStringParam(queryParts, "rev", Revision);
            AddBatchParam(queryParts, "batch", Batch);

            return string.Join("&", queryParts);
        }

        private static void AddStringParam(List<string> queryParts, string paramName, string paramValue)
        {
            if (paramValue == null)
                return;

            queryParts.Add($"{paramName}={paramValue}");
        }

        private static void AddBatchParam(List<string> queryParts, string paramName, bool batch)
        {
            if (!batch)
                return;

            queryParts.Add($"{paramName}=ok");
        }
    }
}
