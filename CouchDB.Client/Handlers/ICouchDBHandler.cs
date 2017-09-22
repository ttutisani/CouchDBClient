﻿using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Represents CouchDB Handler, which is capable of sending raw requests to CouchDB.
    /// </summary>
    public interface ICouchDBHandler
    {
        /// <summary>
        /// Sends request to CouchDB and returns response.
        /// </summary>
        /// <param name="relativeUrl">Relative url to the CouchDB endpoint.</param>
        /// <param name="requestMethod"><see cref="RequestMethod"/> to be used when sending request.</param>
        /// <param name="request">Instance of <see cref="Request"/> to be sent.</param>
        /// <returns>Instance of <see cref="Response"/> received from CouchDB.</returns>
        Task<Response> SendRequestAsync(string relativeUrl, RequestMethod requestMethod, Request request);
    }
}