using System;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB server instance. 
    /// Starting point for all interactions with CouchDB.
    /// </summary>
    public interface ICouchDBServer
    {
        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <param name="dbName">Database name which will be created.</param>
        /// <returns><see cref="Task"/> which can be awaited.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        Task CreateDbAsync(string dbName);

        /// <summary>
        /// Deletes the specified database, 
        /// and all the documents and attachments contained within it.
        /// </summary>
        /// <param name="dbName">Database name to be deleted.</param>
        /// <returns>Awaitable task.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        Task DeleteDbAsync(string dbName);

        /// <summary>
        /// Returns a list of all the databases in the CouchDB instance.
        /// </summary>
        /// <returns>String array containing all database names.</returns>
        Task<string[]> GetAllDbNamesAsync(ListQueryParams queryParams = null);

        /// <summary>
        /// Retrieves instance of <see cref="ICouchDBHandler"/> which can be used to send raw requests to CouchDB.
        /// </summary>
        /// <returns>Instance of <see cref="ICouchDBHandler"/>.</returns>
        ICouchDBHandler GetHandler();

        /// <summary>
        /// Accessing the root of a CouchDB instance returns meta information about the instance. 
        /// The response is a JSON structure (represented as C# object) containing information 
        /// about the server, including a welcome message and the version of the server.
        /// </summary>
        /// <returns><see cref="ServerInfo"/> object containing server metadata information.</returns>
        Task<ServerInfo> GetInfoAsync();

        /// <summary>
        /// Selects specific database for working with documents in it.
        /// </summary>
        /// <param name="dbName">Name of database to be selected.</param>
        /// <returns>Instance of <see cref="ICouchDBDatabase"/>.</returns>
        /// <exception cref="ArgumentNullException">Required parameter is null or empty.</exception>
        ICouchDBDatabase SelectDatabase(string dbName);
    }
}