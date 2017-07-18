using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestApiClientBuilder.Core.Providers;
using RestApiClientBuilder.Core.Tests.TestObjects;

namespace RestApiClientBuilder.Core.Tests
{
    [TestClass]
    public class RestApiClientBuilderTests
    {
        private Uri _baseUri = new Uri("http://localhost-faulted");

        private Mock<IRestConnectionProvider> GetFaultedConnection()
        {
            Mock<IRestConnectionProvider> connectionProviderMock = new Mock<IRestConnectionProvider>();
            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse { IsSuccess = false, StatusCode = 400 });
            return connectionProviderMock;
        }

        private Mock<IRestConnectionProvider> GetSuccessConnection()
        {
            Mock<IRestConnectionProvider> connectionProviderMock = new Mock<IRestConnectionProvider>();
            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse { IsSuccess = true, StatusCode = 200 });
            return connectionProviderMock;
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_No_Arguments_HandlesError()
        {
            Mock<IRestConnectionProvider> connectionProviderMock = GetFaultedConnection();

            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(definition)
                .Get()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Content);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_With_UriArguments_HandlesError()
        {
            Mock<IRestConnectionProvider> connectionProviderMock = GetFaultedConnection();
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/{value}");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(definition)
                .Get()
                .WithUriArgument("id", 100)
                .WithUriArgument("{value}", 101)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Request/100/101"), result.Uri);
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void RestApiClientBuilder_GET_With_UriArguments_TriggersExceptionWhenArgumentMissing()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/{value}");

            RestApiClientBuilder.Build()
                .From(definition)
                .Get()
                .WithUriArgument("id", 100)
                .ExecuteAsync().Wait();
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_With_UriArguments_InTheMiddle_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/Details");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Get()
                .WithUriArgument("id", 100)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Request/100/Details"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_With_QueryArguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Get()
                .WithQueryArgument("model", searchObject)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search?model.page=1&model.pageSize=10&model.subObject.value=1-ABC-123&model.subObject.condition=StartsWith"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_POST_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Post(searchObject)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void RestApiClientBuilder_POST_No_Arguments_ThrowsError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            RestApiClientBuilder.Build()
                .From(definition)
                .Post(null)
                .ExecuteAsync().Wait();
        }

        [TestMethod]
        public void RestApiClientBuilder_POST_With_UriArguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/{second}");

            bool errorHandlerCalled = false;

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Post(searchObject)
                .WithUriArgument("id", 100)
                .WithUriArgument("{second}", 101)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Request/100/101"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_PUT_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Put(searchObject)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void RestApiClientBuilder_PUT_No_Arguments_ThrowsError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            RestApiClientBuilder.Build()
                .From(definition)
                .Put(null)
                .ExecuteAsync().Wait();
        }

        [TestMethod]
        public void RestApiClientBuilder_PUT_With_UriArguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/{second}");

            bool errorHandlerCalled = false;

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Put(searchObject)
                .WithUriArgument("id", 100)
                .WithUriArgument("{second}", 101)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Request/100/101"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_DELETE_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "User");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Delete()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/User"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_DELETE_With_UriArguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "User", "{id}");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .From(definition)
                .Get()
                .WithUriArgument("id", 100)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/User/100"), result.Uri);
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_With_OwnCancellationToken_NotDisposed()
        {
            Mock<IRestConnectionProvider> connectionProviderMock = GetSuccessConnection();

            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(5000);

            bool errorHandlerCalled = false;
            bool successHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .UseConnectionProvider(connectionProviderMock.Object)
                .From(definition)
                .Get()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .OnSuccess(httpCode => { successHandlerCalled = true; })
                .ExecuteAsync(cancellationTokenSource).Result;

            cancellationTokenSource.Cancel();

            Assert.AreEqual(false, errorHandlerCalled);
            Assert.AreEqual(true, successHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri);
        }
    }
}