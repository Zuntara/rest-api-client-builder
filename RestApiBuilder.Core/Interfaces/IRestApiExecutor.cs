using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiExecutor
    {
        /// <summary>
        /// Execute the REST call and return the result of the call in a wrapped object.
        /// </summary>
        /// <returns>Result of the call with error-info included</returns>
        /// <exception cref="ArgumentMissingException">Throwed when an API argument in the uri is missing.</exception>
        Task<RestApiCallResult> ExecuteAsync();
    }
}