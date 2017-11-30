using System;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents Error which can be received in server response.
    /// </summary>
    public sealed class ServerResponseError
    {
        /// <summary>
        /// Initializes new instance of <see cref="ServerResponseError"/> class.
        /// </summary>
        /// <param name="errorString">Raw error string.</param>
        /// <param name="reason">Reason phrase.</param>
        public ServerResponseError(string errorString, string reason = null)
        {
            if (errorString == null)
                throw new ArgumentNullException(nameof(errorString));

            RawError = errorString;

            CommonError parsedCommonError;
            if (Enum.TryParse(errorString, true, out parsedCommonError))
                CommonError = parsedCommonError;

            Reason = reason;
        }

        /// <summary>
        /// If the error is one of the common pre-defined error types, this will get respective value.
        /// </summary>
        public CommonError? CommonError { get; }

        /// <summary>
        /// Gets raw error string.
        /// </summary>
        public string RawError { get; }

        /// <summary>
        /// Gets reason phrase.
        /// </summary>
        public string Reason { get; }

        internal static ServerResponseError FromString(string error, string reason = null)
        {
            return string.IsNullOrWhiteSpace(error) ? null : new ServerResponseError(error, reason);
        }
    }
}
