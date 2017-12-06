using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents base class of query parameters.
    /// </summary>
    public abstract class QueryParams
    {
        /// <summary>
        /// When implemented, converts current instance of <see cref="QueryParams"/> to <see cref="string"/>.
        /// </summary>
        /// <returns></returns>
        ///// <exception cref="NoException"></exception>
        internal abstract string ToQueryString();

        /// <summary>
        /// Append query params to URL.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
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
