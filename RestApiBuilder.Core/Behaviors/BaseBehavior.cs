using System;
using System.Net.Http;
using System.Net.Http.Headers;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Providers;

namespace RestApiClientBuilder.Core.Behaviors
{
    public abstract class BaseBehavior : IRestBehavior
    {
        public virtual void OnRequestCreated(ConnectionRequest request)
        {
        }

        public virtual void OnClientCreation(IRestConnectionProvider provider, Uri baseAddress)
        {
            
        }
    }
}