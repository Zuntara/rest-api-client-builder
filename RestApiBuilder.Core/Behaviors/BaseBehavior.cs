using System;
using System.Net.Http;
using System.Net.Http.Headers;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Providers;

namespace RestApiClientBuilder.Core.Behaviors
{
    /// <summary>
    /// Base class for behaviors
    /// </summary>
    public abstract class BaseBehavior : IRestBehavior
    {
        /// <summary>
        /// Called when a request is created.
        /// </summary>
        /// <param name="request">Request that's created</param>
        public virtual void OnRequestCreated(ConnectionRequest request)
        {
        }

        /// <summary>
        /// Called when a client is being created, typical place to add a handler to the provider for <see cref="IRestConnectionProvider.OnCreateClient"/>
        /// </summary>
        /// <param name="provider">Provider used to create requests and clients</param>
        /// <param name="baseAddress">Base Uri for the requests</param>
        public virtual void OnClientCreation(IBaseRestConnectionProvider provider, Uri baseAddress)
        {
        }
    }

    public class AdaptRequestHeaderBehavior : BaseBehavior
    {
        private readonly string[] _acceptContentTypes;
        private readonly string[] _acceptEncodings;

        public static AdaptRequestHeaderBehavior Create(string[] acceptContentTypes, string[] acceptEncodings)
        {
            return new AdaptRequestHeaderBehavior(acceptContentTypes, acceptEncodings);
        }

        
        private AdaptRequestHeaderBehavior(string[] acceptContentTypes, string[] acceptEncodings)
        {
            _acceptContentTypes = acceptContentTypes;
            _acceptEncodings = acceptEncodings;
        }

        public override void OnRequestCreated(ConnectionRequest request)
        {
            request.HeaderAcceptContentTypes = _acceptContentTypes;
            request.HeaderAcceptEncodings = _acceptEncodings;
        }
    }
}