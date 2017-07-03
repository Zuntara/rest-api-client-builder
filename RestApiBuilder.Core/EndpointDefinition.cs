using System;
using System.Linq;
using System.Reflection;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Utils;

namespace RestApiClientBuilder.Core
{
    public class EndpointDefinition : IRestApiForDefinition
    {
        private EndpointDefinition()
        {
            ApiVersion = "api/I";
        }

        /// <summary>
        /// Gets or sets the API version to call
        /// </summary>
        internal string ApiVersion { get; }

        /// <summary>
        /// Name of the controller in the route
        /// </summary>
        internal string Controller { get; set; }
        /// <summary>
        /// Name of the action in the route with the parameters included. Parameter format: {paramName}.
        /// Example:  
        ///      controller = "User", ActionWithArguments = "Details/{id}"  =>   /api/I/User/Details/{id}
        ///      controller = "User", ActionWithArguments = "{id}"  =>   /api/I/User/{id}
        /// </summary>
        internal string ActionWithArguments { get; set; }

        /// <summary>
        /// Optional base-address to do the calls to
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Internal method for fetching the relative part of the URI
        /// </summary>
        /// <param name="getArgumentName">name of the variable name for the GET request on the rest api.</param>
        /// <param name="getArgumentObject">object to encode in the GET request</param>
        /// <returns>relative part of the rest call</returns>
        /// <exception cref="InvalidOperationException">Throwed when the get-arguments are invalid</exception>
        internal Uri GetUri(string getArgumentName = null, object getArgumentObject = null)
        {
            if ((getArgumentName == null && getArgumentObject != null) || (getArgumentName != null && getArgumentObject == null))
            {
                throw new InvalidOperationException("Both arguments should be empty or filled!");
            }

            string relativeUriString = $"/{ApiVersion}/{Controller}";
            if (!string.IsNullOrWhiteSpace(ActionWithArguments))
            {
                relativeUriString = $"{relativeUriString}/{ActionWithArguments}";
            }

            string postfix = GenerateGetArguments(getArgumentName, getArgumentObject);
            relativeUriString += "?" + postfix;

            return new Uri(relativeUriString.TrimEnd('?'), UriKind.Relative);
        }

        private string GenerateGetArguments(string getArgumentName, object getArgumentObject, string currentRoot = "")
        {
            if (IsSystemType(getArgumentObject?.GetType()))
            {
                return string.Empty;
            }

            string arguments = string.Empty;

            var properties = getArgumentObject?.GetType().GetRuntimeProperties() ?? new PropertyInfo[0];
            foreach (var property in properties.Where(p => p.GetValue(getArgumentObject) != null))
            {
                var value = property.GetValue(getArgumentObject);
                if (property.PropertyType == typeof(string) || property.PropertyType.GetTypeInfo().IsValueType)
                {
                    // end of the chain
                    if (!string.IsNullOrWhiteSpace(currentRoot))
                    {
                        arguments += getArgumentName.ToLower() + "." + currentRoot + "." + property.Name.FirstLetterToLower() + "=" + value + "&";
                    }
                    else
                    {
                        arguments += getArgumentName.ToLower() + "." + property.Name.FirstLetterToLower() + "=" + value + "&";
                    }
                }
                else if (value != null)
                {
                    // drill down
                    string subArguments = GenerateGetArguments(getArgumentName, value, property.Name.FirstLetterToLower());
                    arguments += subArguments + "&";
                }

            }
            return arguments.TrimEnd('&');
        }

        private bool IsSystemType(Type type)
        {
            if (string.IsNullOrWhiteSpace(type?.Namespace)) return true;

            if (type.Namespace.StartsWith("System.") || type.Namespace.StartsWith("Microsoft."))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Factory method for building a <see cref="EndpointDefinition"/>
        /// </summary>
        /// <param name="baseAddress">Base address to use</param>
        /// <param name="controller">Controller name in the path</param>
        /// <returns></returns>
        public static EndpointDefinition Build(Uri baseAddress, string controller)
        {
            return new EndpointDefinition
            {
                BaseAddress = baseAddress,
                ActionWithArguments = string.Empty,
                Controller = controller,
            };
        }

        /// <summary>
        /// Factory method for building a <see cref="EndpointDefinition"/>
        /// </summary>
        /// <param name="baseAddress">Base address to use</param>
        /// <param name="controller">Controller name in the path</param>
        /// <param name="actionWithArguments">Action name with arguments if there</param>
        /// <returns></returns>
        public static EndpointDefinition Build(Uri baseAddress, string controller, string actionWithArguments)
        {
            return new EndpointDefinition
            {
                BaseAddress = baseAddress,
                ActionWithArguments = actionWithArguments,
                Controller = controller,
            };
        }

        /// <summary>
        /// Define a GET operation
        /// </summary>
        /// <returns>fluent api for additional parameters</returns>
        public IRestApiGetDefinition Get()
        {
            return RestApiClientBuilder.BuildFor(BaseAddress).From(this).Get();
        }

        /// <summary>
        /// Define a POST operation
        /// </summary>
        /// <param name="objectToPost">object to JSON encode</param>
        /// <returns>fluent api for additional parameters</returns>
        public IRestApiMethodDefinition Post(object objectToPost)
        {
            return RestApiClientBuilder.BuildFor(BaseAddress).From(this).Post(objectToPost);
        }

        /// <summary>
        /// Define a PUT operation
        /// </summary>
        /// <param name="objectToPut">object to JSON encode</param>
        /// <returns>fluent api for additional parameters</returns>
        public IRestApiMethodDefinition Put(object objectToPut)
        {
            return RestApiClientBuilder.BuildFor(BaseAddress).From(this).Put(objectToPut);
        }

        /// <summary>
        /// Define a DELETE operation
        /// </summary>
        /// <returns>fluent api for additional parameters</returns>
        public IRestApiMethodDefinition Delete()
        {
            return RestApiClientBuilder.BuildFor(BaseAddress).From(this).Delete();
        }
    }
}