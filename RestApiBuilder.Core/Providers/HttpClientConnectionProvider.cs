using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Implementation that uses a HttpClient object to do the connections
    /// </summary>
    public class HttpClientConnectionProvider : BaseConnectionProvider<HttpClient>
    {
        private static HttpClient _httpClient;

        /// <summary>
        /// Initializes a new HttpClient connection provider
        /// </summary>
        public HttpClientConnectionProvider()
        {
            OnCreateClient = (hasHandlers) =>
            {
                var client = new HttpClient();
                HasHandlers = false;
                return client;
            };
        }

        public override void ConfigureHeaders(ConnectionRequest connectionRequest, HttpClient client)
        {
            if (connectionRequest.HeaderAcceptContentTypes != null)
            {
                client.DefaultRequestHeaders.Accept.Clear();
                foreach (string acceptContentType in connectionRequest.HeaderAcceptContentTypes)
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptContentType));
                }
            }

            if (connectionRequest.HeaderAcceptEncodings != null)
            {
                client.DefaultRequestHeaders.AcceptEncoding.Clear();
                foreach (string acceptEncoding in connectionRequest.HeaderAcceptEncodings)
                {
                    client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(acceptEncoding));
                }
            }
        }

        private void ConfigureHeaders(ConnectionRequest connectionRequest, HttpContent content)
        {
            if (connectionRequest.HeaderAcceptContentTypes != null)
            {
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(connectionRequest.HeaderAcceptContentTypes.First());
            }

            if (connectionRequest.HeaderAcceptEncodings != null)
            {
                content.Headers.ContentEncoding.Clear();
                foreach (string acceptEncoding in connectionRequest.HeaderAcceptEncodings)
                {
                    content.Headers.ContentEncoding.Add(acceptEncoding);
                }
            }
        }

        /// <summary>
        /// Called when a request is created
        /// </summary>
        /// <param name="method">HTTP method to invoke</param>
        /// <param name="baseAddress">Base Uri to invoke to</param>
        /// <param name="endpointRelativeUri">Relative uri for the endpoint</param>
        /// <param name="content">Optional content for body of PUT and POST requests</param>
        /// <returns>Connection request summary data</returns>
        public override ConnectionRequest CreateRequest(HttpMethod method, Uri baseAddress, Uri endpointRelativeUri, string content)
        {
            return new ConnectionRequest
            {
                Method = method,
                BaseAddress = baseAddress,
                RelativeUri = endpointRelativeUri,
                Content = content
            };
        }

        /// <summary>
        /// Executes a request in an async manner.
        /// </summary>
        /// <param name="connectionRequest">Request definition to execute</param>
        /// <param name="token">Cancellation token to use when executing</param>
        /// <returns>Response object of the request</returns>
        public override async Task<ConnectionRequestResponse> ProcessRequestAsync(ConnectionRequest connectionRequest, CancellationToken token)
        {
            _httpClient = (HttpClient)OnCreateClient(HasHandlers);

            ConfigureHeaders(connectionRequest, _httpClient);

            _httpClient.BaseAddress = connectionRequest.BaseAddress;

            if (connectionRequest.Method == HttpMethod.Post)
            {
                HttpContent content = new StringContent(connectionRequest.Content);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, connectionRequest.RelativeUri);
                request.Content = content;

                ConfigureHeaders(connectionRequest, request.Content);

                HttpResponseMessage response = await ExecuteHttpCallAsync(token, request);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = await ResolveErrorReasonAsync(response)
                };
            }
            if (connectionRequest.Method == HttpMethod.Put)
            {
                HttpContent content = new StringContent(connectionRequest.Content);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, connectionRequest.RelativeUri);
                request.Content = content;

                ConfigureHeaders(connectionRequest, request.Content);

                HttpResponseMessage response = await ExecuteHttpCallAsync(token, request);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = await ResolveErrorReasonAsync(response)
                };
            }
            if (connectionRequest.Method == HttpMethod.Get)
            {
                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, connectionRequest.RelativeUri);

                HttpResponseMessage response = await ExecuteHttpCallAsync(token, HttpCompletionOption.ResponseContentRead, request);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = await ResolveErrorReasonAsync(response)
                };
            }
            if (connectionRequest.Method == HttpMethod.Delete)
            {
                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, connectionRequest.RelativeUri);

                HttpResponseMessage response = await ExecuteHttpCallAsync(token, request);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = await ResolveErrorReasonAsync(response)
                };
            }
            return null;
        }

        public virtual async Task<HttpResponseMessage> ExecuteHttpCallAsync(CancellationToken token, HttpRequestMessage request)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request, token);
            return response;
        }

        public virtual async Task<HttpResponseMessage> ExecuteHttpCallAsync(CancellationToken token, HttpCompletionOption httpCompletionOption, HttpRequestMessage request)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request, httpCompletionOption, token);
            return response;
        }

        private async Task<string> ResolveErrorReasonAsync(HttpResponseMessage response)
        {
            return !response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
        }
    }
}