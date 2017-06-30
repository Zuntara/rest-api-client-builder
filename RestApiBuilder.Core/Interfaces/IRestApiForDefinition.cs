namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiForDefinition
    {
        /// <summary>
        /// Define a GET operation
        /// </summary>
        /// <returns>fluent api for additional parameters</returns>
        IRestApiGetDefinition Get();

        /// <summary>
        /// Define a POST operation
        /// </summary>
        /// <param name="objectToPost">object to JSON encode</param>
        /// <returns>fluent api for additional parameters</returns>
        IRestApiMethodDefinition Post(object objectToPost);

        /// <summary>
        /// Define a PUT operation
        /// </summary>
        /// <param name="objectToPut">object to JSON encode</param>
        /// <returns>fluent api for additional parameters</returns>
        IRestApiMethodDefinition Put(object objectToPut);

        /// <summary>
        /// Define a DELETE operation
        /// </summary>
        /// <returns>fluent api for additional parameters</returns>
        IRestApiMethodDefinition Delete();
    }
}