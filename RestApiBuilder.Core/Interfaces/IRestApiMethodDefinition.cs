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
        IRestApiExecutor OnErrorResponse(Action<HttpStatusCode> onErrorResponseHandler);
    }
}