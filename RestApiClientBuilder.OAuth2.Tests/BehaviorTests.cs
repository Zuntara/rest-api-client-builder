﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestApiClientBuilder.Core;
using RestApiClientBuilder.Core.Providers;
using HttpMethod = RestApiClientBuilder.Core.HttpMethod;

namespace RestApiClientBuilder.OAuth2.Tests
{
    [TestClass]
    public class BehaviorTests
    {
        [TestMethod]
        public void TestBehaviorSuccess()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();

            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse() { IsSuccess = true, StatusCode = 200 });

            bool calledOnSuccess = false;

            Core.RestApiClientBuilder.Build()
                .Behavior(OAuth2ClientCredentialsBehavior.Create(new ClientCredentialSettings
                {
                    TokenEndpointUri = new Uri("http://localhost:4545/auth/token"),
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                }))
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(EndpointDefinition.Build(new Uri("http://localhost:4545"), "Regions"))
                .Get()
                .OnSuccess(c => { calledOnSuccess = true; } )
                .ExecuteAsync().Wait();

            connectionProviderMock.Verify(c => c.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
            connectionProviderMock.Verify(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsTrue(calledOnSuccess);
        }

        [TestMethod]
        public void TestBehaviorError()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();

            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse { IsSuccess = false, StatusCode = 400 });

            bool calledOnError = false;

            Core.RestApiClientBuilder.Build()
                .Behavior(OAuth2ClientCredentialsBehavior.Create(new ClientCredentialSettings
                {
                    TokenEndpointUri = new Uri("http://localhost:4545/auth/token"),
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                }))
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(EndpointDefinition.Build(new Uri("http://localhost:4545"), "Regions"))
                .Get()
                .OnError(c => { calledOnError = true; })
                .ExecuteAsync().Wait();

            connectionProviderMock.Verify(c => c.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
            connectionProviderMock.Verify(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsTrue(calledOnError);
        }

        [TestMethod]
        public void TestBehaviorTimeout()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();

            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .Throws<TaskCanceledException>();

            bool calledOnTimeout = false;

            Core.RestApiClientBuilder.Build()
                .Behavior(OAuth2ClientCredentialsBehavior.Create(new ClientCredentialSettings
                {
                    TokenEndpointUri = new Uri("http://localhost:4545/auth/token"),
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                }))
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(EndpointDefinition.Build(new Uri("http://localhost:4545"), "Regions"))
                .Get()
                .OnTimeout(() => { calledOnTimeout = true; })
                .ExecuteAsync(10).Wait();

            connectionProviderMock.Verify(c => c.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
            connectionProviderMock.Verify(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsTrue(calledOnTimeout);
        }

        [TestMethod]
        public void TestBehaviorTimeoutWithToken()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();

            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .Throws<TaskCanceledException>();

            bool calledOnTimeout = false;

            Core.RestApiClientBuilder.Build()
                .Behavior(OAuth2ClientCredentialsBehavior.Create(new ClientCredentialSettings
                {
                    TokenEndpointUri = new Uri("http://localhost:4545/auth/token"),
                    ClientId = "client-id",
                    ClientSecret = "client-secret"
                }))
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(EndpointDefinition.Build(new Uri("http://localhost:4545"), "Regions"))
                .Get()
                .OnTimeout(() => { calledOnTimeout = true; })
                .ExecuteAsync(new CancellationTokenSource(10)).Wait();

            connectionProviderMock.Verify(c => c.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
            connectionProviderMock.Verify(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsTrue(calledOnTimeout);
        }

    }
}
