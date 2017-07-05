using System;
using System.Net.Http;
using System.Net.Http.Headers;
using DotNetOpenAuth.OAuth2;
using RestApiClientBuilder.Core.Behaviors;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Providers;

namespace RestApiClientBuilder.OAuth2
{
    public class OAuth2ClientCredentialsBehavior : BaseBehavior
    {
        private readonly ClientCredentialSettings _settings;

        /// <summary>
        /// Creates a new instance of the behavior that handles OAuth2 client credentials
        /// </summary>
        /// <param name="settings">Settings needed for the behavior</param>
        /// <returns></returns>
        public static IRestBehavior Create(ClientCredentialSettings settings)
        {
            return new OAuth2ClientCredentialsBehavior(settings);
        }

        private OAuth2ClientCredentialsBehavior(ClientCredentialSettings settings)
        {
            _settings = settings;
        }

        public override void OnClientCreation(IRestConnectionProvider provider, Uri baseAddress)
        {
            if (provider is HttpClientConnectionProvider)
            {
                provider.OnCreateClient = (hasHandlers) =>
                {
                    if (hasHandlers) return null;

                    UserAgentClient userAgent = GetUserAgent();
                    IAuthorizationState authorizationCode = userAgent.GetClientAccessToken();

                    var client = new HttpClient(userAgent.CreateAuthorizingHandler(authorizationCode));
                    client.BaseAddress = baseAddress;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    provider.HasHandlers = true;

                    return client;
                };
            }
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
