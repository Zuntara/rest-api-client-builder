using System;

namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Connection request wrapper
    /// </summary>
    public class ConnectionRequest
    {
        /// <summary>
        /// Creates a new instance of a connection request
        /// </summary>
        public ConnectionRequest()
        {
            HeaderAcceptContentTypes = new[] { "application/json" };
            HeaderAcceptEncodings = new[] { "utf-8" };
        }

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

        /// <summary>
        /// Request accept headers (application/json = default)
        /// </summary>
        public string[] HeaderAcceptContentTypes { get; set; }

        /// <summary>
        /// Header encoding accept headers (utf-8 = default)
        /// </summary>
        public string[] HeaderAcceptEncodings { get; set; }
    }
}