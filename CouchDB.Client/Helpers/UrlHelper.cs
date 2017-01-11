﻿using System;

namespace CouchDB.Client
{
    internal static class UrlHelper
    {
        public static string CombineUrl(string prefix, string suffix)
        {
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix))
                return string.Concat(prefix, suffix);

            if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(suffix))
                throw new ArgumentException("White space strings cannot be combined as URL.");

            return $"{prefix.TrimEnd('/')}/{suffix.TrimStart('/')}";
        }
    }
}