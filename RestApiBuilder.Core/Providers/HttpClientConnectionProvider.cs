using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Providers
{
    /// <summary>
    /// Implementation that uses a HttpClient object to do the connections
    /// </summary>
    public class HttpClientConnectionProvider : BaseConnectionProvider
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
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HasHandlers = false;
                return client;
            };
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

            _httpClient.BaseAddress = connectionRequest.BaseAddress;

            if (connectionRequest.Method == HttpMethod.Post)
            {
                HttpContent content = new StringContent(connectionRequest.Content);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, connectionRequest.RelativeUri);
                request.Content = content;

                HttpResponseMessage response = await _httpClient.SendAsync(request, token);
                
                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int) response.StatusCode,
                    ErrorReason = !response.IsSuccessStatusCode ? response.ReasonPhrase : null
                };
            }
            if (connectionRequest.Method == HttpMethod.Put)
            {
                HttpContent content = new StringContent(connectionRequest.Content);

                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Put, connectionRequest.RelativeUri);
                request.Content = content;

                HttpResponseMessage response = await _httpClient.SendAsync(request, token);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = !response.IsSuccessStatusCode ? response.ReasonPhrase : null
                };
            }
            if (connectionRequest.Method == HttpMethod.Get)
            {
                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get,  connectionRequest.RelativeUri);

                HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, token);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = !response.IsSuccessStatusCode ? response.ReasonPhrase : null
                };
            }
            if (connectionRequest.Method == HttpMethod.Delete)
            {
                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Delete, connectionRequest.RelativeUri);

                HttpResponseMessage response = await _httpClient.SendAsync(request, token);

                return new ConnectionRequestResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseString = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null,
                    StatusCode = (int)response.StatusCode,
                    ErrorReason = !response.IsSuccessStatusCode ? response.ReasonPhrase : null
                };
            }
            return null;
        }
    }
}