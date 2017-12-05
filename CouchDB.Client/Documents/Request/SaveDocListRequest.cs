using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Client
{
    internal sealed class SaveDocListRequest
    {
        private readonly List<JObject> _documents = new List<JObject>();

        /// <summary>
        /// Initializes new instance of <see cref="SaveDocListRequest"/> class.
        /// </summary>
        /// <param name="newEdits"></param>
        /// <exception cref="NoException"></exception>
        public SaveDocListRequest(bool newEdits)
        {
            NewEdits = newEdits;
        }

        public bool NewEdits { get; }

        /// <summary>
        /// Adds document json strings to corrent instance of <see cref="SaveDocListRequest"/>.
        /// </summary>
        /// <param name="documents"></param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public void AddDocuments(string[] documents)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            _documents.AddRange(documents.Select(doc => JObject.Parse(doc)));
        }

        /// <summary>
        /// Converts current <see cref="SaveDocListRequest"/> into <see cref="JObject"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NoException"></exception>
        public JObject ToJson()
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
