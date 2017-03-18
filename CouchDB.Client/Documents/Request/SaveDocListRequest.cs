using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Client
{
    internal sealed class SaveDocListRequest
    {
        private readonly List<JObject> _documents = new List<JObject>();

        public SaveDocListRequest(bool newEdits)
        {
            NewEdits = newEdits;
        }

        public bool NewEdits { get; }

        internal void AddDocuments(string[] documents)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            _documents.AddRange(documents.Select(doc => JObject.Parse(doc)));
        }

        internal JObject ToJson()
        {
            var json = JObject.FromObject(
                new
                {
                    new_edits = NewEdits,
                    docs = JArray.FromObject(_documents)
                });
            return json;
        }
    }
}
