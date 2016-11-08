using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB server instance. 
    /// Starting point for all interactions with CouchDB.
    /// </summary>
    public sealed class CouchDBServer : IDisposable
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBServer"/> class.
        /// </summary>
        /// <param name="baseUrl">(Required) base URL for CouchDB (e.g. "http://localhost:5984/")</param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="FormatException"><paramref name="baseUrl"/> is not in valid format.</exception>
        public CouchDBServer(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            Uri serverUri;
            if (!Uri.TryCreate(UrlHelper.CombineUrl(baseUrl, "/"), UriKind.Absolute, out serverUri))
                throw new FormatException("URL is not in valid format.");

            _http = new HttpClient();
            _http.BaseAddress = serverUri;
        }

        /// <summary>
        /// Disposes <see cref="CouchDBServer"/> instance, after which it becomes unusable.
        /// </summary>
        public void Dispose()
        {
            _http.Dispose();
        }

        #region GetInfo

        /// <summary>
        /// Accessing the root of a CouchDB instance returns meta information about the instance. 
        /// The response is a JSON structure (represented as C# object) containing information 
        /// about the server, including a welcome message and the version of the server.
        /// </summary>
        /// <returns><see cref="ServerInfo"/> object containing server metadata information.</returns>
        public async Task<ServerInfo> GetInfoAsync()
        {
            var infoHttpResponse = await _http.GetAsync(string.Empty);
            var infoDTO = await HttpClientHelper.HandleResponse<ServerInfoDTO>(infoHttpResponse);
            var serverInfo = new ServerInfo(infoDTO);

            return serverInfo;
        }

        internal sealed class ServerInfoDTO
        {
            public string CouchDB { get; set; }

            public string Version { get; set; }

            public VendorInfoDTO Vendor { get; set; }

            public sealed class VendorInfoDTO
            {
                public string Name { get; set; }
            }
        }

        #endregion

        /// <summary>
        /// Returns a list of all the databases in the CouchDB instance.
        /// </summary>
        /// <returns>String array containing all database names.</returns>
        public async Task<string[]> GetAllDbNamesAsync(QueryParams queryParams = null)
        {
            var allDbsQuery = QueryParams.AppendQueryParams("_all_dbs", queryParams);

            var dbNamesHttpResponse = await _http.GetAsync(allDbsQuery);
            var dbNamesArray = await HttpClientHelper.HandleResponse<string[]>(dbNamesHttpResponse);
            
            return dbNamesArray;
        }

        #region CreateDb

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="dbName">Database name which will be created.</param>
        /// <returns><see cref="Task"/> which can be awaited.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task CreateDbAsync(string dbName)
        { 
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            var createDbHttpResponse = await _http.PutAsync(dbName, null);
            await HttpClientHelper.HandleResponse(createDbHttpResponse);
        }

        internal sealed class ServerResponseDTO
        {
            public bool OK { get; set; }

            public ServerResponseError Error { get; set; }

            public string Reason { get; set; }
        }

        #endregion

        /// <summary>
        /// Deletes the specified database, 
        /// and all the documents and attachments contained within it.
        /// </summary>
        /// <param name="dbName">Database name to be deleted.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public async Task DeleteDbAsync(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            var deleteHttpResponse = await _http.DeleteAsync(dbName);
            await HttpClientHelper.HandleResponse(deleteHttpResponse);
        }

        /// <summary>
        /// Selects specific database for working with documents in it.
        /// </summary>
        /// <param name="dbName">Name of database to be selected.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        public CouchDBDatabase SelectDatabase(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            return new CouchDBDatabase(UrlHelper.CombineUrl(_http.BaseAddress.OriginalString, dbName));
        }
    }
}
