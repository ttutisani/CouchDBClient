namespace CouchDB.Client
{
    /// <summary>
    /// Defines list of possible server errors in CouchDB.
    /// </summary>
    public enum CommonError
    {
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
        Illegal_DocId,

        /// <summary>
        /// Document format validation failed. e.g. if _id is passed with upper case property name, such as _Id.
        /// </summary>
        Doc_Validation
    }

    /// <summary>
    /// Defines extension methods over <see cref="CommonError"/> enumeration type.
    /// </summary>
    public static class CommonErrorExtensions
    {
        /// <summary>
        /// Compares <see cref="CommonError"/> with <see cref="System.String"/> error.
        /// </summary>
        /// <param name="this">Instance of <see cref="CommonError"/> value.</param>
        /// <param name="error">Error string to compare with.</param>
        /// <returns>True if values equal, otherwise false.</returns>
        public static bool EqualsErrorString(this CommonError @this, string error)
        {
            return @this.ToString().Equals(error, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns string representation of <see cref="CommonError"/> value.
        /// </summary>
        /// <param name="this">Instance of <see cref="CommonError"/> value.</param>
        /// <returns>String representation of the given value.</returns>
        public static string ToErrorString(this CommonError @this)
        {
            return @this.ToString().ToLower();
        }
    }
}
