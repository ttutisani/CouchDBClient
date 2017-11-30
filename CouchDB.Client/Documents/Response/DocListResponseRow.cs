using System;
using Newtonsoft.Json.Linq;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents row in the document list response.
    /// </summary>
    /// <typeparam name="TDocument">Type of document.</typeparam>
    public sealed class DocListResponseRow<TDocument>
    {
        /// <summary>
        /// Initializes new instance of <see cref="DocListResponseRow{TDocument}"/> class.
        /// </summary>
        /// <param name="id">ID of the row.</param>
        /// <param name="key">Key of the row.</param>
        /// <param name="value"><see cref="DocListResponseRowValue"/> of the row, which holds revision.</param>
        /// <param name="document">Document.</param>
        /// <param name="error">Error information (if any).</param>
        public DocListResponseRow(string id, string key, DocListResponseRowValue value, TDocument document, ServerResponseError error)
        {
            Id = id;
            Key = key;
            Value = value;
            Document = document;
            Error = error;
        }

        /// <summary>
        /// Gets ID of the row.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets Key of the row.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets <see cref="DocListResponseRowValue"/> of the row, which holds revision.
        /// </summary>
        public DocListResponseRowValue Value { get; }

        /// <summary>
        /// Gets document.
        /// </summary>
        public TDocument Document { get; }

        /// <summary>
        /// Gets error information (if any).
        /// </summary>
        public ServerResponseError Error { get; }

        internal DocListResponseRow<TResult> Cast<TResult>(Func<TDocument, TResult> converter)
        {
            return new DocListResponseRow<TResult>(Id, Key, Value, converter(Document), Error);
        }

        internal static DocListResponseRow<string> FromJsonToString(JObject json)
        {
            var id = SerializationHelper.GetStringOrDefault(json, "id");
            var key = SerializationHelper.GetStringOrDefault(json, "key");
            var value = SerializationHelper.GetObjectOrDefault(json, "value");
            var doc = SerializationHelper.GetObjectOrDefault(json, "doc")?.ToString();
            var error = SerializationHelper.GetStringOrDefault(json, "error");

            return new DocListResponseRow<string>(id, key, DocListResponseRowValue.FromJson(value), doc, ServerResponseError.FromString(error));
        }
    }
}
