using System;
using System.Net.Http;
using System.Net.Http.Headers;
using RestApiClientBuilder.Core.Interfaces;

namespace RestApiClientBuilder.Core.Behaviors
{
    public abstract class BaseBehavior : IRestBehavior
    {
        public virtual void OnRequestCreated(HttpRequestMessage request)
        {
        }

        public virtual void OnClientConfiguration(ref HttpClient client, Uri baseAddress)
        {
            client.BaseAddress = baseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}