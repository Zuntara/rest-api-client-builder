using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;
using RestApiClientBuilder.Core.Behaviors;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Providers;

namespace RestApiClientBuilder.OAuth2
{
    /// <summary>
    /// Implements the client credential flow of OAuth2 for the Auth0 provider
    /// </summary>
    public class Auth0ClientCredentialBehavior : BaseBehavior
    {
        private readonly ClientCredentialSettingsAuth0 _settings;

        /// <summary>
        /// Creates a new instance of the behavior that handles OAuth2 client credentials
        /// </summary>
        /// <param name="settings">Settings needed for the behavior</param>
        /// <returns></returns>
        public static IRestBehavior Create(ClientCredentialSettingsAuth0 settings)
        {
            return new Auth0ClientCredentialBehavior(settings);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OAuth2ClientCredentialsBehavior"/>
        /// </summary>
        /// <param name="settings"></param>
        private Auth0ClientCredentialBehavior(ClientCredentialSettingsAuth0 settings)
        {
            _settings = settings;
        }

        public override void OnClientCreation(IBaseRestConnectionProvider provider, Uri baseAddress)
        {
            if (provider is HttpClientConnectionProvider)
            {
                provider.OnCreateClient = (hasHandlers) =>
                {
                    if (hasHandlers) return null;

                    UserAgentClient userAgent = GetUserAgent();

                    AuthenticationApiClient auth0Client = new AuthenticationApiClient(_settings.TokenEndpointUri);
                    var tokenTask = auth0Client.GetTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Audience = _settings.Audience,
                        ClientId = _settings.ClientId,
                        ClientSecret = _settings.ClientSecret
                    });
                    tokenTask.Wait();

                    IAuthorizationState authorizationCode = new AuthorizationState();
                    authorizationCode.AccessToken = tokenTask.Result.AccessToken;
                    authorizationCode.AccessTokenExpirationUtc = DateTime.UtcNow.AddSeconds(tokenTask.Result.ExpiresIn);
                    authorizationCode.AccessTokenIssueDateUtc = DateTime.UtcNow;
                    authorizationCode.RefreshToken = tokenTask.Result.RefreshToken;
                    
                    if (provider is IRestConnectionProvider<HttpClient>)
                    {
                        var client = new HttpClient(userAgent.CreateAuthorizingHandler(authorizationCode));
                        client.BaseAddress = baseAddress;
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        provider.HasHandlers = true;

                        return client;
                    }
                    throw new NotSupportedException("Clients other then HttpClient is not (yet) supported.");
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