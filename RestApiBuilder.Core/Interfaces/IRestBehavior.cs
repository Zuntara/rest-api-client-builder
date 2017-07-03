using System;
using System.Net.Http;

namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestBehavior
    {
        void OnRequestCreated(HttpRequestMessage request);

        void OnClientConfiguration(ref HttpClient client, Uri baseAddress);

    }
}