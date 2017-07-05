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
using RestApiClientBuilder.Core.Providers;
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
            BaseConnectionProvider connectionProvider = new HttpClientConnectionProvider();
            return new ApiBuilder(null, connectionProvider);
        }

        /// <summary>
        /// Build a rest API interface, the base-uri taken from the parameter
        /// </summary>
        /// <param name="baseAddress">Base address to call the API</param>
        /// <returns>Fluent interface for building the request</returns>
        public static IRestApiBuildOperation BuildFor(Uri baseAddress)
        {
            BaseConnectionProvider connectionProvider = new HttpClientConnectionProvider();
            return new ApiBuilder(baseAddress, connectionProvider);
        }

        /// <summary>
        /// Builder class for creating the REST call
        /// </summary>
        private sealed class ApiBuilder : IBuilderOperations
        {
            private IRestConnectionProvider _connectionProvider;
            private Uri _baseAddress;
            private EndpointDefinition _endpointDefinition;
            private HttpMethod _httpMethod;
            private Action<int> _errorHandler;
            private Action<int> _successHandler;
            private Action _timeoutHandler;
            private Dictionary<string, object> _uriArguments;
            private object _getQueryArgument;
            private string _getArgumentName;
            private object _bodyObject;

            private HttpClient _client;

            private readonly List<IRestBehavior> _behaviors = new List<IRestBehavior>();

            public ApiBuilder(Uri baseAddress, BaseConnectionProvider connectionProvider)
            {
                _connectionProvider = connectionProvider;
                if (baseAddress != null)
                {
                    _baseAddress = baseAddress;
                }
            }

            public IRestApiForDefinition From(EndpointDefinition definition)
            {
                _endpointDefinition = definition;
                if (_baseAddress == null)
                {
                    _baseAddress = definition.BaseAddress;
                }
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

            public IRestApiExecutor OnError(Action<int> onErrorResponseHandler)
            {
                if (_errorHandler != null)
                {
                    throw new InvalidOperationException("OnError already registered.");
                }
                _errorHandler = onErrorResponseHandler;
                return this;
            }

            public IRestApiExecutor OnSuccess(Action<int> onSuccessResponseHandler)
            {
                if (_successHandler != null)
                {
                    throw new InvalidOperationException("OnSuccess already registered.");
                }
                _successHandler = onSuccessResponseHandler;
                return this;
            }

            public IRestApiExecutor OnTimeout(Action onTimeoutHandler)
            {
                if (_timeoutHandler != null)
                {
                    throw new InvalidOperationException("OnTimeout already registered.");
                }
                _timeoutHandler = onTimeoutHandler;
                return this;
            }

            public async Task<RestApiCallResult> ExecuteAsync(int timeoutMs = 5000)
            {
                RestApiCallResult callResult = new RestApiCallResult();

                Stopwatch timer = Stopwatch.StartNew();

                _behaviors.Foreach(b => b.OnClientCreation(_connectionProvider, _baseAddress));

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeoutMs);

                ConnectionRequestResponse response;

                try
                {
                    switch (_httpMethod)
                    {
                        case HttpMethod.Get:
                            response = await ProcessGetRequestAsync(callResult, cancellationTokenSource);
                            break;
                        case HttpMethod.Post:
                            response = await ProcessPostRequestAsync(callResult, cancellationTokenSource);
                            break;
                        case HttpMethod.Put:
                            response = await ProcessPutRequestAsync(callResult, cancellationTokenSource);
                            break;
                        case HttpMethod.Delete:
                            response = await ProcessDeleteRequestAsync(callResult, cancellationTokenSource);
                            break;
                        default:
                            throw new InvalidOperationException("HTTP Method not configured properly");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    _errorHandler?.Invoke(0);
                    callResult.Errors.Add(httpEx.InnerException?.Message ?? httpEx.Message);
                    callResult.Speed = timer.Elapsed;
                    return callResult;
                }

                if (response == null)
                {
                    _timeoutHandler?.Invoke();
                    callResult.Errors.Add($"The request is canceled. (timeout = {timeoutMs} ms)");
                    callResult.Speed = timer.Elapsed;
                    return callResult;
                }

                if (!response.IsSuccess)
                {
                    _errorHandler?.Invoke(response.StatusCode);
                    callResult.Errors.Add(response.ErrorReason);
                    callResult.Speed = timer.Elapsed;
                    return callResult;
                }

                _successHandler?.Invoke(response.StatusCode);

                callResult.IsSucceeded = true;
                callResult.Content = response.ResponseString;
                callResult.Speed = timer.Elapsed;

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

            IRestApiExecutor IRestApiMethodDefinition.Behavior(IRestBehavior behavior)
            {
                if (!_behaviors.Contains(behavior))
                {
                    _behaviors.Add(behavior);
                }

                return this;
            }

            public IRestApiBuildOperation UseConnectionProvider(IRestConnectionProvider provider)
            {
                _connectionProvider = provider;
                return this;
            }

            // Helper methods

            private async Task<ConnectionRequestResponse> ProcessPostRequestAsync(RestApiCallResult callResult, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();
                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                string jsonSerialized = JsonConvert.SerializeObject(_bodyObject);

                var connectionRequest = _connectionProvider.CreateRequest(HttpMethod.Post, _baseAddress, endpointRelativeUri, jsonSerialized);
                _behaviors.Foreach(b => b.OnRequestCreated(connectionRequest));

                try
                {
                    ConnectionRequestResponse response = await _connectionProvider.ProcessRequestAsync(connectionRequest, cancellationTokenSource.Token);
                    return response;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }

            private async Task<ConnectionRequestResponse> ProcessPutRequestAsync(RestApiCallResult callResult, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();
                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                string jsonSerialized = JsonConvert.SerializeObject(_bodyObject);

                var connectionRequest = _connectionProvider.CreateRequest(HttpMethod.Put, _baseAddress, endpointRelativeUri, jsonSerialized);
                _behaviors.Foreach(b => b.OnRequestCreated(connectionRequest));

                try
                {
                    ConnectionRequestResponse response = await _connectionProvider.ProcessRequestAsync(connectionRequest, cancellationTokenSource.Token);
                    return response;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }

            private async Task<ConnectionRequestResponse> ProcessGetRequestAsync(RestApiCallResult callResult, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri(_getArgumentName, _getQueryArgument);

                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                ConnectionRequest connectionRequest = _connectionProvider.CreateRequest(HttpMethod.Get, _baseAddress, endpointRelativeUri, null);
                _behaviors.Foreach(b => b.OnRequestCreated(connectionRequest));

                try
                {
                    ConnectionRequestResponse response = await _connectionProvider.ProcessRequestAsync(connectionRequest, cancellationTokenSource.Token);
                    return response;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }

            private async Task<ConnectionRequestResponse> ProcessDeleteRequestAsync(RestApiCallResult callResult, CancellationTokenSource cancellationTokenSource)
            {
                Uri endpointRelativeUri = _endpointDefinition.GetUri();

                endpointRelativeUri = FillUriParameters(endpointRelativeUri);
                callResult.Uri = new Uri(_baseAddress, endpointRelativeUri);

                var connectionRequest = _connectionProvider.CreateRequest(HttpMethod.Delete, _baseAddress, endpointRelativeUri, null);
                _behaviors.Foreach(b => b.OnRequestCreated(connectionRequest));

                try
                {
                    ConnectionRequestResponse response = await _connectionProvider.ProcessRequestAsync(connectionRequest, cancellationTokenSource.Token);
                    return response;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
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