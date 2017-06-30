namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestApiGetDefinition : IRestApiMethodDefinition
    {
        /// <summary>
        /// Supply an object to be parsed into the GET uri with a given prefix as variable name.<br/><br/>
        /// Example: <br/>
        ///     When you give "model" as variable name, ans supply an object with property "ValueObj1" in it.<br/>
        ///     Then the parsed result will be :  model.valueObj1=the_value_in_the_property.<br/><br/>
        /// Empty properties are excluded.
        /// </summary>
        /// <param name="modelVariableName">variable name in the REST API</param>
        /// <param name="valueInQueryParams">object with all properties to be encoded in it</param>
        /// <returns></returns>
        IRestApiGetDefinition WithQueryArgument(string modelVariableName, object valueInQueryParams);
    }
}