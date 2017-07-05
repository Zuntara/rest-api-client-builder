namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiBuildOperation
    {
        /// <summary>
        /// Specify which definition to use for building a call
        /// Default timeout is 5000 ms.
        /// </summary>
        /// <param name="definition">Definition with basic info in it</param>
        /// <returns>fluent api for constructing the rest of the call</returns>
        IRestApiForDefinition From(EndpointDefinition definition);

        /// <summary>
        /// Defines a behavior for intercepting the building of the call.
        /// For example to add OAuth2 security
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        IRestApiBuildOperation Behavior(IRestBehavior behavior);
    }
}