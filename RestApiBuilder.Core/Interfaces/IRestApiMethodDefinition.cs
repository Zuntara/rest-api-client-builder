using System;
using System.Net;

namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiMethodDefinition : IRestApiExecutor
    {
        /// <summary>
        /// Supply an argument defined in the <see cref="EndpointDefinition"/> object on the <see cref="EndpointDefinition.ActionWithArguments"/> property.
        /// This arguments are supplied without the braces unlike in the EndpointDefinition.
        /// </summary>
        /// <param name="name">name of the argument</param>
        /// <param name="value">runtime value of the argument</param>
        /// <returns>Fluent definition for method specification</returns>
        IRestApiMethodDefinition WithUriArgument(string name, object value);

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

        /// <summary>
        /// Defines a behavior for intercepting the building of the call.
        /// For example to add OAuth2 security
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        IRestApiExecutor Behavior(IRestBehavior behavior);
    }
}