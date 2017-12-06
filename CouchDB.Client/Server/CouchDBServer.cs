using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB server instance. 
    /// Starting point for all interactions with CouchDB.
    /// </summary>
    public sealed class CouchDBServer : ICouchDBServer
    {
        private readonly string _baseUrl;

        /// <summary>
        /// Initializes new instance of <see cref="CouchDBServer"/> class.
        /// </summary>
        /// <param name="baseUrl">(Required) base URL for CouchDB (e.g. "http://localhost:5984/")</param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="FormatException"><paramref name="baseUrl"/> is not in valid format.</exception>
        public CouchDBServer(string baseUrl)
            : this(new CouchDBHandler(baseUrl))
        {
            _baseUrl = baseUrl;
        }

        private readonly ICouchDBHandler _handler;

        /// <summary>
        /// Initializes new instance of <see cref=" CouchDBServer"/> class.
        /// </summary>
        /// <param name="handler">CouchDB Handler.</param>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        internal CouchDBServer(ICouchDBHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handler = handler;
        }

        #region GetInfo

        /// <summary>
        /// Accessing the root of a CouchDB instance returns meta information about the instance. 
        /// The response is a JSON structure (represented as C# object) containing information 
        /// about the server, including a welcome message and the version of the server.
        /// </summary>
        /// <returns><see cref="ServerInfo"/> object containing server metadata information.</returns>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task<ServerInfo> GetInfoAsync()
        {
            ServerInfo serverInfo = null;

            var response = await _handler.SendRequestAsync(string.Empty, RequestMethod.GET, Request.Empty).Safe();
            if (response != null)
            {
                var infoDTO = await response.ReadAsAsync<ServerInfoDTO>(false).Safe();
                if (infoDTO != null)
                    serverInfo = new ServerInfo(infoDTO);
            }
            return serverInfo;
        }

        /// <summary>
        /// Information about CouchDB server.
        /// </summary>
        public sealed class ServerInfoDTO
        {
            /// <summary>
            /// Gets or sets welcome message from CouchDB.
            /// </summary>
            public string CouchDB { get; set; }

            /// <summary>
            /// Gets or sets version of CouchDB server.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Gets or sets CouchDB vendor information.
            /// </summary>
            public VendorInfoDTO Vendor { get; set; }

            /// <summary>
            /// CouchDB vendor information.
            /// </summary>
            public sealed class VendorInfoDTO
            {
                /// <summary>
                /// Gets or sets name of CouchDB server vendor.
                /// </summary>
                public string Name { get; set; }
            }
        }

        #endregion

        /// <summary>
        /// Returns a list of all the databases in the CouchDB instance.
        /// </summary>
        /// <returns>String array containing all database names.</returns>
        /// <exception cref="CouchDBClientException">Error response receive from CouchDB server.</exception>
        public async Task<string[]> GetAllDbNamesAsync(ListQueryParams queryParams = null)
        {
            string[] dbNames = null;

            var allDbsQuery = QueryParams.AppendQueryParams("_all_dbs", queryParams);
            var response = await _handler.SendRequestAsync(allDbsQuery, RequestMethod.GET, Request.Empty).Safe();

            if (response != null)
                dbNames = await response.ReadAsAsync<string[]>(false).Safe();

            return dbNames;
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="dbName">Database name which will be created.</param>
        /// <returns><see cref="Task"/> which can be awaited.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task CreateDbAsync(string dbName)
        { 
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            var response = await _handler.SendRequestAsync(dbName, RequestMethod.PUT, Request.Empty).Safe();
            if (response != null)
                await response.EnsureSuccessAsync(false).Safe();
        }

        /// <summary>
        /// Deletes the specified database, 
        /// and all the documents and attachments contained within it.
        /// </summary>
        /// <param name="dbName">Database name to be deleted.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Delete request was already sent.</exception>
        /// <exception cref="CouchDBClientException">Error response received from CouchDB server.</exception>
        public async Task DeleteDbAsync(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            var response = await _handler.SendRequestAsync(dbName, RequestMethod.DELETE, Request.Empty).Safe();
            if (response != null)
                await response.EnsureSuccessAsync(true);
        }

        /// <summary>
        /// Selects specific database for working with documents in it.
        /// </summary>
        /// <param name="dbName">Name of database to be selected.</param>
        /// <returns>Instance of <see cref="ICouchDBDatabase"/>.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        /// <exception cref="FormatException">Resulting URL with <paramref name="dbName"/> was not in valid format.</exception>
        public ICouchDBDatabase SelectDatabase(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            return new CouchDBDatabase(UrlHelper.CombineUrl(_baseUrl, dbName));
        }

        /// <summary>
        /// Retrieves instance of <see cref="ICouchDBHandler"/> which can be used to send raw requests to CouchDB.
        /// </summary>
        /// <returns>Instance of <see cref="ICouchDBHandler"/>.</returns>
        ///// <exception cref="NoException"></exception>
        public ICouchDBHandler GetHandler()
        {
            return _handler;
        }
    }
}
