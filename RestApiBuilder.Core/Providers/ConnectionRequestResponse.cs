namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Response of request wrapper
    /// </summary>
    public class ConnectionRequestResponse
    {
        /// <summary>
        /// Response in string format (raw)
        /// </summary>
        public string ResponseString { get; set; }

        /// <summary>
        /// Statuscode of the response
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// True when the call is a success
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Filled when an error occured.
        /// </summary>
        public string ErrorReason { get; set; }
    }
}