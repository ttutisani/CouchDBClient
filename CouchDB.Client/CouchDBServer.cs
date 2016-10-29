using System;
using System.Net.Http;
using Newtonsoft.Json;
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
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out serverUri))
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
        public async Task<ServerInfo> GetInfo()
        {
            var serverInfoJsonString = await _http.GetStringAsync(string.Empty);
            var serverInfoDTO = JsonConvert.DeserializeObject<ServerInfoDTO>(serverInfoJsonString);

            var serverInfo = new ServerInfo(serverInfoDTO);
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
        public async Task<string[]> GetAllDbNames()
        {
            var dbNamesJson = await _http.GetStringAsync("_all_dbs");
            var dbNamesArray = JsonConvert.DeserializeObject<string[]>(dbNamesJson);
            return dbNamesArray;
        }

        #region CreateDb

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="dbName">Database name which will be created.</param>
        /// <returns><see cref="Task"/> which can be awaited.</returns>
        public async Task CreateDb(string dbName)
        { 
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            var httpResponse = await _http.PutAsync(dbName, null);
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorMessage = $"Http status code '{httpResponse.StatusCode}', Http reason phrase '{httpResponse.ReasonPhrase}'.";

                var responseJson = await httpResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    throw new CouchDBClientException(errorMessage, null, null);
                }

                var responseObject = JsonConvert.DeserializeObject<ServerResponseDTO>(responseJson);
                throw new CouchDBClientException(errorMessage, new ServerResponse(responseObject), null);
            }
            
            //All's cool. Carry on.
        }

        internal sealed class ServerResponseDTO
        {
            public bool OK { get; set; }

            public ServerResponseError Error { get; set; }

            public string Reason { get; set; }
        }

        #endregion
    }
}
