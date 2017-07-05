using System;
using System.Net;
using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiExecutor
    {
        /// <summary>
        /// Execute the REST call and return the result of the call in a wrapped object.
        /// </summary>
        /// <param name="timeoutMs">Optional timeout in milliseconds. Default 5000 ms</param>
        /// <returns>Result of the call with error-info included</returns>
        /// <exception cref="ArgumentMissingException">Throwed when an API argument in the uri is missing.</exception>
        Task<RestApiCallResult> ExecuteAsync(int timeoutMs = 5000);

        /// <summary>
        /// Gets triggered when the statuscode of the call is different then 200
        /// </summary>
        /// <param name="onErrorResponseHandler">Handler to execute when not 200/></param>
        /// <returns>fluent interface for execution</returns>
        IRestApiExecutor OnError(Action<HttpStatusCode> onErrorResponseHandler);

        /// <summary>
        /// Gets triggered when the statuscode of the call is equal then 200
        /// </summary>
        /// <param name="onSuccessResponseHandler">Handler to execute when 200/></param>
        /// <returns>fluent interface for execution</returns>
        IRestApiExecutor OnSuccess(Action<HttpStatusCode> onSuccessResponseHandler);

        /// <summary>
        /// Gets triggered when the statuscode of the call is equal then 200
        /// </summary>
        /// <param name="onTimeoutHandler">Handler to execute when the request timed out/></param>
        /// <returns>fluent interface for execution</returns>
        IRestApiExecutor OnTimeout(Action onTimeoutHandler);
    }
}