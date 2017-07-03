using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestApiClientBuilder.Core;

namespace RestApiClientBuilder.OAuth2.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestApi()
        {
            Core.RestApiClientBuilder.Build()
                .Behavior(OAuth2ClientCredentialsBehavior.Create(new ClientCredentialSettings
                {
                    TokenEndpointUri = new Uri("http://localhost:4545/auth/token"),
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                }))
                .From(EndpointDefinition.Build(new Uri("http://localhost:4545"), "Regions"))
                .Get()
                .ExecuteAsync().Wait();
        }
    }
}
