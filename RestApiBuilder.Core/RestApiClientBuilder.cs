using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Utils;

namespace RestApiClientBuilder.Core
{
    /// <summary>
    /// This class will build REST API calls based on a <see cref="EndpointDefinition"/>
    /// </summary>
    public static class RestApiClientBuilder
    {
        /// <summary>
        /// Build a rest API interface, the base-uri is extracted from the <see cref="EndpointDefinition"/>
        /// </summary>
        /// <returns>Fluent interface for building the request</returns>
        public static IRestApiBuildOperation Build()
        {
            return new ApiBuilder(null);
        }

        /// <summary>
        /// Build a rest API interface, the base-uri taken from the parameter
        /// </summary>
        /// <param name="baseAddress">Base address to call the API</param>
        /// <returns>Fluent interface for building the request</returns>
        public static IRestApiBuildOperation BuildFor(Uri baseAddress)
        {
            return new ApiBuilder(baseAddress);
        }

        /// <summary>
        /// Builder class for creating the REST call
        /// </summary>
        private sealed class ApiBuilder : IBuilderOperations
        {
            private Uri _baseAddress;
            private EndpointDefinition _endpointDefinition;
            private HttpMethod _httpMethod;
            private Action<HttpStatusCode> _errorHandler;
            private Dictionary<string, object> _uriArguments;
            private object _getQueryArgument;
            private string _getArgumentName;
            private int _callTimeOut;
            private object _bodyObject;

            private HttpClient _client;

            private readonly List<IRestBehavior> _behaviors = new List<IRestBehavior>();

            public ApiBuilder(Uri baseAddress)
            {
                if (baseAddress != null)
                {
                    _baseAddress = baseAddress;
                }
            }

            public IRestApiForDefinition From(EndpointDefinition definition)
            {
                _endpointDefinition = definition;
                _callTimeOut = 5000;
                if (_baseAddress == null)
                {
                    _baseAddress = definition.BaseAddress;
                }
                return this;
            }

            public IRestApiForDefinition From(EndpointDefinition definition, int timeoutMs)
            {
                From(definition);
                _callTimeOut = timeoutMs;
                return this;
            }

            public IRestApiGetDefinition Get()
            {
                _httpMethod = HttpMethod.Get;
                return this;
            }

            public IRestApiMethodDefinition Post(object objectToPost)
            {
                if (objectToPost == null)
                {
                    throw new ArgumentNullException(nameof(objectToPost));
                }
                _httpMethod = HttpMethod.Post;
                _bodyObject = objectToPost;
                return this;
            }

            public IRestApiMethodDefinition Put(object objectToPut)
            {
                if (objectToPut == null)
                {
                    throw new ArgumentNullException(nameof(objectToPut));
                }
                _httpMethod = HttpMethod.Put;
                _bodyObject = objectToPut;
                return this;
            }

            public IRestApiMethodDefinition Delete()
            {
                _httpMethod = HttpMethod.Delete;
                return this;
            }

            public IRestApiExecutor OnErrorResponse(Action<HttpStatusCode> onErrorResponseHandler)
            {
                _errorHandler = onErrorResponseHandler;
                return this;
            }

            public async Task<RestApiCallResult> ExecuteAsync()
            {
                RestApiCallResult callResult = new RestApiCallResult();

                Stopwatch timer = Stopwatch.StartNew();
                using (_client = new HttpClient())
                {
                    _client.BaseAddress = _baseAddress;
                    _client.DefaultRequestHeaders.Accept.Clear();
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    _behaviors.Foreach(b => b.OnClientConfiguration(ref _client, _baseAddress));

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(_callTimeOut);

                    HttpResponseMessage response;

                    try
                    {
                        switch (_httpMethod)
                        {
                            case HttpMethod.Get:
                                response = await ProcessGetRequestAsync(callResult, _client, cancellationTokenSource);
                                break;
                            case HttpMethod.Post:
                                response = await ProcessPostRequestAsync(callResult, _client, cancellationTokenSource);
                                break;
                            case HttpMethod.Put:
                                response = await ProcessPutRequestAsync(callResult, _client, cancellationTokenSource);
                                break;
                            case HttpMethod.Delete:
                                response = await ProcessDeleteRequestAsync(callResult, _client, cancellationTokenSource);
                                break;
                            default:
                                throw new InvalidOperationException("HTTP Method not configured properly");
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        _errorHandler?.Invoke(HttpStatusCode.Gone);
                        callResult.Errors.Add(httpEx.InnerException?.Message ?? httpEx.Message);
                        callResult.Speed = timer.Elapsed;
                        return callResult;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _errorHandler?.Invoke(response.StatusCode);
                        callResult.Errors.Add(response.ReasonPhrase);
                        callResult.Speed = timer.Elapsed;
                        return callResult;
                    }

                    callResult.IsSucceeded = true;
                    callResult.Content = await response.Content.ReadAsStringAsync();
                    callResult.Speed = timer.Elapsed;
                }
                return callResult;
            }

            public IRestApiMethodDefinition WithUriArgument(string name, object value)
            {
                if (_uriArguments == null)
                {
                    _uriArguments = new Dictionary<string, object>();
                }
                _uriArguments[name] = value;
                return this;
            }

            public IRestApiGetDefinition WithQueryArgument(string modelVariableName, object valueInQueryParams)
            {
                if (_getQueryArgument != null)
                {
                    throw new NotSupportedException("You can only provide 1 query object");
                }

                if ((modelVariableName == null || valueInQueryParams == null))
                {
                    throw new InvalidOperationException("Both arguments should be filled!");
                }

                _getQueryArgument = valueInQueryParams;
                _getArgumentName = modelVariableName;
                return this;
            }

            public IRestApiBuildOperation Behavior(IRestBehavior behavior)
            {
                if (!_behaviors.Contains(behavior))
                {
                    _behaviors.Add(behavior);
                }

                return this;
            }

            // Helper methods

            private async Task<HttpResponseMessage> ProcessPostRequestAsync(RestApiCallResult callResult, HttpClient client, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();
                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                string jsonSerialized = JsonConvert.SerializeObject(_bodyObject);
                HttpContent content = new StringContent(jsonSerialized);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, endpointRelativeUri);
                request.Content = content;
                _behaviors.Foreach(b => b.OnRequestCreated(request));
                var response = await client.SendAsync(request, cancellationTokenSource.Token);

                return response;
            }

            private async Task<HttpResponseMessage> ProcessPutRequestAsync(RestApiCallResult callResult, HttpClient client, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();
                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                string jsonSerialized = JsonConvert.SerializeObject(_bodyObject);
                HttpContent content = new StringContent(jsonSerialized);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, endpointRelativeUri);
                request.Content = content;
                _behaviors.Foreach(b => b.OnRequestCreated(request));
                var response = await client.SendAsync(request, cancellationTokenSource.Token);

                return response;
            }

            private async Task<HttpResponseMessage> ProcessGetRequestAsync(RestApiCallResult callResult, HttpClient client, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri(_getArgumentName, _getQueryArgument);

                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, endpointRelativeUri);
                _behaviors.Foreach(b => b.OnRequestCreated(request));
                
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead,
                    cancellationTokenSource.Token);

                //var response = await client.GetAsync(endpointRelativeUri, HttpCompletionOption.ResponseContentRead, cancellationTokenSource.Token);
                return response;
            }

            private async Task<HttpResponseMessage> ProcessDeleteRequestAsync(RestApiCallResult callResult, HttpClient client, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();

                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, endpointRelativeUri);
                _behaviors.Foreach(b => b.OnRequestCreated(request));
                var response = await client.SendAsync(request, cancellationTokenSource.Token);
                return response;
            }

            private Uri FillUriParameters(Uri endpointRelativeUri)
            {
                if (_uriArguments == null || !_uriArguments.Any())
                {
                    return endpointRelativeUri;
                }

                string textUri = endpointRelativeUri.ToString();

                if (Regex.Matches(textUri, "{*[A-z]*.}").Count != _uriArguments.Count)
                {
                    throw new ArgumentMissingException("Not all arguments are given");
                }

                foreach (var uriArgument in _uriArguments)
                {
                    string formattedKey = uriArgument.Key.Trim('{', '}');
                    textUri = textUri.Replace($"{{{formattedKey}}}", uriArgument.Value?.ToString());
                }

                Uri replacedUri = new Uri(textUri, UriKind.Relative);
                return replacedUri;
            }
        }
    }

    
}