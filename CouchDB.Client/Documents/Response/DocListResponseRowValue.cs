using Newtonsoft.Json.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents value within document list response row.
    /// </summary>
    public sealed class DocListResponseRowValue
    {
        internal DocListResponseRowValue(string revision)
        {
            Revision = revision;
        }

        /// <summary>
        /// Gets revision of the document.
        /// </summary>
        public string Revision { get; }

        private sealed class ValueDTO
        {
            public string Rev { get; set; }
        }

        internal static DocListResponseRowValue FromJson(JObject value)
        {
            return value == null ? null : new DocListResponseRowValue(value.ToObject<ValueDTO>().Rev);
        }
    }
}
