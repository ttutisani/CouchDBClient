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
        File_Exists,

        /// <summary>
        /// Bad request was sent to CouchDB server, which cannot be handled successfully.
        /// </summary>
        Bad_Request,

        /// <summary>
        /// Conflict while executing operation. e.g. revision number did not match.
        /// </summary>
        Conflict,

        /// <summary>
        /// Object cannot be found on the server. e.g. database or document does not exist.
        /// </summary>
        Not_Found,

        /// <summary>
        /// Data sent to server has invalid content type, i.e. "Content-Type" header has wrong value in it.
        /// </summary>
        Bad_Content_Type,

        /// <summary>
        /// Doc ID value supplied by the client is not valid.
        /// </summary>
        Illegal_DocId
    }
}
