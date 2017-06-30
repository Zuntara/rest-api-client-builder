using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RestApiClientBuilder.Core
{
    public class RestApiCallResult
    {
        public RestApiCallResult()
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// List of optional errors when they occured.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Indication that all went well
        /// </summary>
        public bool IsSucceeded { get; set; }

        /// <summary>
        /// Contains the JSON retrieved from the call
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The URI that is requested
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets the speed of the call
        /// </summary>
        public TimeSpan Speed { get; set; }

        /// <summary>
        /// Parse the given json in the <see cref="Content"/> property.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public TModel Parse<TModel>()
        {
            return JsonConvert.DeserializeObject<TModel>(Content);
        }

        public string GetErrorMessage()
        {
            return string.Join("\r\n", Errors);
        }
    }
}