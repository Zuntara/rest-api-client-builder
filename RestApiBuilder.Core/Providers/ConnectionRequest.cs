using System;

namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Connection request wrapper
    /// </summary>
    public class ConnectionRequest
    {
        /// <summary>
        /// Optional content for PUT or POST requests
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Relative Uri for the request
        /// </summary>
        public Uri RelativeUri { get; set; }

        /// <summary>
        /// Method of the request
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Base Uri for the request
        /// </summary>
        public Uri BaseAddress { get; set; }
    }
}