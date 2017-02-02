using System;
using Newtonsoft.Json.Linq;

namespace CouchDB.Client
{
    public sealed class DocListResponseRow<TDocument>
    {
        internal DocListResponseRow(string id, string key, DocListResponseRowValue value, TDocument document, ServerResponseError error)
        {
            Id = id;
            Key = key;
            Value = value;
            Document = document;
            Error = error;
        }

        public string Id { get; }

        public string Key { get; }

        public DocListResponseRowValue Value { get; }

        public TDocument Document { get; }

        public ServerResponseError Error { get; }

        internal DocListResponseRow<TResult> Cast<TResult>(Func<TDocument, TResult> converter)
        {
            return new DocListResponseRow<TResult>(Id, Key, Value, converter(Document), Error);
        }

        internal static DocListResponseRow<JObject> FromJson(JObject json)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DocListResponseRowValue
    {
        internal DocListResponseRowValue(string revision)
        {
            Revision = revision;
        }

        public string Revision { get; }
    }
}
