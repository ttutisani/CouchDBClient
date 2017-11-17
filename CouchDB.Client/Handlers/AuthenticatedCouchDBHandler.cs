using System;

namespace CouchDB.Client
{
    internal sealed class AuthenticatedCouchDBHandler : HttpCouchDBHandler
    {
        private static int OneIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? 1 : 0;
        }

        private static string InjectCredentialsIntoBaseUrl(string username, string password, string baseUrl)
        {
            var emptyCount = OneIfEmpty(username) + OneIfEmpty(password);
            if (emptyCount == 1)
                throw new ArgumentException("Incorrect username or password. Supply either both empty or both non empty.");

            Uri baseUrlAsUri;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out baseUrlAsUri))
                return baseUrl;

            var baseUrlWithCredentialsBuilder = new UriBuilder(baseUrlAsUri);
            if (emptyCount == 0)
            {
                baseUrlWithCredentialsBuilder.UserName = username;
                baseUrlWithCredentialsBuilder.Password = password;
            }
            if (baseUrlWithCredentialsBuilder.Port == 80)
                baseUrlWithCredentialsBuilder.Port = -1;

            return baseUrlWithCredentialsBuilder.ToString();
        }

        public AuthenticatedCouchDBHandler(string username, string password, string baseUrl, IStatelessHttpClient http)
            : base(InjectCredentialsIntoBaseUrl(username, password, baseUrl), http)
        {

        }
    }
}
