using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestApiClientBuilder.Core.Tests.TestObjects;

namespace RestApiClientBuilder.Core.Tests
{
    [TestClass]
    public class RestApiClientBuilderTests
    {
        private Uri _baseUri = new Uri("http://localhost-faulted");

        [TestMethod]
        public void RestApiClientBuilder_GET_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .With(definition)
                .Get()
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Request/{id}/{value}");

            bool errorHandlerCalled = false;

            var result = RestApiClientBuilder.Build()
                .With(definition)
                .Get()
                .WithUriArgument("id", 100)
                .WithUriArgument("{value}", 101)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
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
                .With(definition)
                .Get()
                .WithUriArgument("id", 100)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
                .Get()
                .WithQueryArgument("model", searchObject)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
                .Post(searchObject)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
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
                .With(definition)
                .Post(searchObject)
                .WithUriArgument("id", 100)
                .WithUriArgument("{second}", 101)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
                .Put(searchObject)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
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
                .With(definition)
                .Put(searchObject)
                .WithUriArgument("id", 100)
                .WithUriArgument("{second}", 101)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
                .Delete()
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
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
                .With(definition)
                .Get()
                .WithUriArgument("id", 100)
                .OnErrorResponse(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/User/100"), result.Uri);
        }
    }
}