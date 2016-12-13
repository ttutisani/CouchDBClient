using System;

namespace CouchDB.Client
{
    public abstract class QueryParams
    {
        internal abstract string ToQueryString();

        internal static string AppendQueryParams(string url, QueryParams queryParams)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            if (queryParams == null)
                return url;

            var query = queryParams.ToQueryString();
            if (string.IsNullOrWhiteSpace(query))
                return url;

            var qmarkIndex = url.IndexOf('?');
            if (qmarkIndex < 0)
                return $"{url}?{query}";

            if (qmarkIndex == url.Length - 1)
                return $"{url}{query}";

            return $"{url}&{query}";
        }
    }
}
