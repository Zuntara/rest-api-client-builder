using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Defines an interface for a connection provider
    /// </summary>
    public interface IRestConnectionProvider
    {
        /// <summary>
        /// True when a custom interception handler is added to the client
        /// </summary>
        bool HasHandlers { get; set; }

        /// <summary>
        /// Called when the client needs to be created
        /// </summary>
        Func<bool, object> OnCreateClient { get; set; }

        /// <summary>
        /// Called when a request is created
        /// </summary>
        /// <param name="method">HTTP method to invoke</param>
        /// <param name="baseAddress">Base Uri to invoke to</param>
        /// <param name="endpointRelativeUri">Relative uri for the endpoint</param>
        /// <param name="content">Optional content for body of PUT and POST requests</param>
        /// <returns>Connection request summary data</returns>
        ConnectionRequest CreateRequest(HttpMethod method, Uri baseAddress, Uri endpointRelativeUri, string content);

        /// <summary>
        /// Executes a request in an async manner.
        /// </summary>
        /// <param name="connectionRequest">Request definition to execute</param>
        /// <param name="token">Cancellation token to use when executing</param>
        /// <returns>Response object of the request</returns>
        Task<ConnectionRequestResponse> ProcessRequestAsync(ConnectionRequest connectionRequest, CancellationToken token);
    }
}