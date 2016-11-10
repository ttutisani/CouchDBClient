using System;

namespace CouchDB.Client
{
    internal abstract class QueryParams
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

        #region Delegate based

        private static QueryParams FromDelegate(Func<string> toQueryString)
        {
            return new DelegateBasedQueryParams(toQueryString);
        }

        private sealed class DelegateBasedQueryParams : QueryParams
        {
            private readonly Func<string> _toQueryString;

            internal DelegateBasedQueryParams(Func<string> toQueryString)
            {
                if (toQueryString == null)
                    throw new ArgumentNullException(nameof(toQueryString));

                _toQueryString = toQueryString;
            }

            internal override string ToQueryString()
            {
                return _toQueryString();
            }
        }

        #endregion

        public static implicit operator QueryParams(ListQueryParams from)
        {
            if (from == null)
                return null;

            return FromDelegate(from.ToQueryString);
        }

        public static implicit operator QueryParams(DocQueryParams from)
        {
            if (from == null)
                return null;

            return FromDelegate(from.ToQueryString);
        }
    }
}
