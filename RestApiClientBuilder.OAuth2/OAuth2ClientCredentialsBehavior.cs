using System;
using System.Net.Http;
using DotNetOpenAuth.OAuth2;
using RestApiClientBuilder.Core.Behaviors;
using RestApiClientBuilder.Core.Interfaces;

namespace RestApiClientBuilder.OAuth2
{
    public class OAuth2ClientCredentialsBehavior : BaseBehavior
    {
        private readonly ClientCredentialSettings _settings;

        public static IRestBehavior Create(ClientCredentialSettings settings)
        {
            return new OAuth2ClientCredentialsBehavior(settings);
        }

        private OAuth2ClientCredentialsBehavior(ClientCredentialSettings settings)
        {
            _settings = settings;
        }

        public override void OnClientConfiguration(ref HttpClient client, Uri baseAddress)
        {
            UserAgentClient userAgent = GetUserAgent();

            IAuthorizationState authorizationCode = userAgent.GetClientAccessToken();

            client = new HttpClient(userAgent.CreateAuthorizingHandler(authorizationCode));
          
            base.OnClientConfiguration(ref client, baseAddress);
        }

        private AuthorizationServerDescription GetAuthServerDescription()
        {
            var server = new AuthorizationServerDescription();
            server.TokenEndpoint = _settings.TokenEndpointUri;
            server.ProtocolVersion = ProtocolVersion.V20;
            return server;
        }

        private UserAgentClient GetUserAgent()
        {
            var server = GetAuthServerDescription();
            var client = new UserAgentClient(server, _settings.ClientId);
            client.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(_settings.ClientSecret);
            return client;
        }
    }


}
