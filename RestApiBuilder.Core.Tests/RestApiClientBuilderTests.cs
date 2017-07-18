using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestApiClientBuilder.Core.Behaviors;
using RestApiClientBuilder.Core.Providers;
using RestApiClientBuilder.Core.Tests.TestObjects;

namespace RestApiClientBuilder.Core.Tests
{
    [TestClass]
    public class RestApiClientBuilderTests
    {
        private Uri _baseUri = new Uri("http://localhost-faulted");

        private Mock<IRestConnectionProvider<HttpClient>> GetFaultedConnection()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();
            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse { IsSuccess = false, StatusCode = 400 });
            return connectionProviderMock;
        }

        private Mock<IRestConnectionProvider<HttpClient>> GetSuccessConnection()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = new Mock<IRestConnectionProvider<HttpClient>>();
            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConnectionRequestResponse { IsSuccess = true, StatusCode = 200 });
            return connectionProviderMock;
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_No_Arguments_HandlesError()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = GetFaultedConnection();

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
        public void RestApiClientBuilder_GET_No_Arguments_HasCustomHeaders()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = GetFaultedConnection();

            connectionProviderMock.Setup(c => c.ProcessRequestAsync(It.IsAny<ConnectionRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ConnectionRequest, CancellationToken>((conn, ct) =>
                {
                    connectionProviderMock.Object.ConfigureHeaders(conn, null);
                }).ReturnsAsync(new ConnectionRequestResponse {IsSuccess = false, StatusCode = 400});

            connectionProviderMock.Setup(p => p.CreateRequest(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<Uri>(), It.IsAny<string>()))
                .Returns(new ConnectionRequest());

            bool configureCalled = false;
            connectionProviderMock.Setup(p => p.ConfigureHeaders(It.IsAny<ConnectionRequest>(), It.IsAny<HttpClient>())).Callback<ConnectionRequest, object>(
                (cr, client) =>
                {
                    Assert.AreEqual("application/xml", cr.HeaderAcceptContentTypes[0]);
                    Assert.AreEqual("utf-16", cr.HeaderAcceptEncodings[0]);
                    configureCalled = true;
                });

            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .UseConnectionProvider(connectionProviderMock.Object)
                .Behavior(AdaptRequestHeaderBehavior.Create(new[] { "application/xml" }, new[] { "utf-16" }))
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
            Assert.IsTrue(configureCalled);
        }

        [TestMethod]
        public void RestApiClientBuilder_GET_With_UriArguments_HandlesError()
        {
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = GetFaultedConnection();
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
            Mock<IRestConnectionProvider<HttpClient>> connectionProviderMock = GetSuccessConnection();

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

    [TestClass]
    public class RestApiScenarioTest
    {
        private TrailerDto GetToUpdate()
        {
            return new TrailerDto
            {
                Comment = "",
                LicencePlate = "Q-ABP-080",
                RouteDays = new List<RouteDayDto>(),
                ScheduledOnRouteId = null,
                Tarra = 7510,
                TrailerRestrictions = new List<TrailerRestrictionDto>(),
                UnavailableDays = new List<UnavailableDayDto>
                {
                    new UnavailableDayDto {Date = new DateTime(19,7,2017)},
                    new UnavailableDayDto {Date = new DateTime(20,7,2017)}
                }
            };
        }

        [TestMethod]
        public void TestUpdate()
        {
            
        }

        public enum CountryCode
        {
            BELUX
        }

        public class TrailerDto
        {
            public string ScheduledOnRouteId { get; set; }

            public string LicencePlate { get; set; }

            //public TrailerType Type { get; set; }

            public int Tarra { get; set; }

            public List<TrailerRestrictionDto> TrailerRestrictions { get; set; }

            public string Comment { get; set; }

            public List<UnavailableDayDto> UnavailableDays { get; set; }

            public List<RouteDayDto> RouteDays { get; set; }
        }


        public class TrailerRestrictionDto
        {
            public CountryCode CountryCode { get; set; }

            public int MaxWeight { get; set; }

            public int MaxLength { get; set; }

            public int MaxWidth { get; set; }

            public List<string> ProductGroups { get; set; }
        }

        public class UnavailableDayDto
        {
            public DateTime? Date { get; set; }

            public string Remark { get; set; }
        }


        public class RouteDayDto
        {
            public DateTime Date { get; set; }

            public string RouteId { get; set; }
        }
    }
}