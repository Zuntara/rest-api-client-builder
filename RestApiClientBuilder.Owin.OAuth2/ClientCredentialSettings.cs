using System;

namespace RestApiClientBuilder.OAuth2
{
    /// <summary>
    /// Settings for the OAuth2 <see cref="OAuth2ClientCredentialsBehavior"/>
    /// </summary>
    public class ClientCredentialSettings
    {
        /// <summary>
        /// Gets or sets the Token endpoint for the OAuth2 client credentials handler
        /// </summary>
        public Uri TokenEndpointUri { get; set; }

        /// <summary>
        /// Gets or sets the client-id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client-secret
        /// </summary>
        public string ClientSecret { get; set; }
    }
}