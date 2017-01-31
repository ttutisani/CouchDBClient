using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents Error which can be received in server response.
    /// </summary>
    public sealed class ServerResponseError
    {
        internal ServerResponseError(string errorString)
        {
            if (errorString == null)
                throw new ArgumentNullException(nameof(errorString));

            RawError = errorString;

            CommonError parsedCommonError;
            if (Enum.TryParse(errorString, true, out parsedCommonError))
                CommonError = parsedCommonError;
        }

        /// <summary>
        /// If the error is one of the common pre-defined error types, this will get respective value.
        /// </summary>
        public CommonError? CommonError { get; }

        /// <summary>
        /// Gets raw error string.
        /// </summary>
        public string RawError { get; }
    }
}
