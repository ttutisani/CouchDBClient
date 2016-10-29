namespace CouchDB.Client
{
    /// <summary>
    /// Defines list of possible server errors in CouchDB.
    /// </summary>
    public enum ServerResponseError
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies situation when something already exists and cannot be overwritten.
        /// e.g. when trying to create a database with name that already exists.
        /// </summary>
        File_Exists
    }
}
