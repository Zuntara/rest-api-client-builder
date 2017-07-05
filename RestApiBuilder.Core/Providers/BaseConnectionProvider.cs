using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RestApiClientBuilder.Core.Providers
{
    public class ConnectionRequest
    {
        public string Content { get; set; }
        public Uri RelativeUri { get; set; }
        public HttpMethod Method { get; set; }
        public Uri BaseAddress { get; set; }
    }

    public class ConnectionRequestResponse
    {
        public string ResponseString { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorReason { get; set; }
    }

    public abstract class BaseConnectionProvider : IRestConnectionProvider
    {
        public bool HasHandlers { get; set; }

        public Func<bool, object> OnCreateClient { get; set; }

        public abstract ConnectionRequest CreateRequest(HttpMethod post, Uri baseAddress, Uri endpointRelativeUri, string content);
        public abstract Task<ConnectionRequestResponse> ProcessRequestAsync(ConnectionRequest connectionRequest, CancellationToken token);
    }

    public interface IRestConnectionProvider
    {
        bool HasHandlers { get; set; }

        Func<bool, object> OnCreateClient { get; set; }

        ConnectionRequest CreateRequest(HttpMethod post, Uri baseAddress, Uri endpointRelativeUri, string content);

        Task<ConnectionRequestResponse> ProcessRequestAsync(ConnectionRequest connectionRequest, CancellationToken token);
    }

    public class HttpClientConnectionProvider : BaseConnectionProvider
    {
        private static HttpClient _httpClient;
        
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