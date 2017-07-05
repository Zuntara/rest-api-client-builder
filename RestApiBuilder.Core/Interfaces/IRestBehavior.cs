using System;
using System.Net.Http;
using RestApiClientBuilder.Core.Providers;

namespace RestApiClientBuilder.Core.Interfaces
{
    public interface IRestBehavior
    {
        void OnRequestCreated(ConnectionRequest request);

        void OnClientCreation(IRestConnectionProvider provider, Uri baseAddress);
    }
}