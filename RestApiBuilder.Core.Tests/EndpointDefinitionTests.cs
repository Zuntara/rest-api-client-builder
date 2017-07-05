using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RestApiClientBuilder.Core.Interfaces;
using RestApiClientBuilder.Core.Tests.TestObjects;

namespace RestApiClientBuilder.Core.Tests
{
    [TestClass]
    public class EndpointDefinitionTests
    {
        private Uri _baseUri = new Uri("http://localhost-faulted");

        [TestMethod]
        public void Build_Without_Arguments()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            Assert.IsNotNull(definition);
            Assert.AreEqual("Routes", definition.Controller);
            Assert.AreEqual("Search", definition.ActionWithArguments);
            Assert.AreEqual("api/I", definition.ApiVersion);

            Uri resultUri = definition.GetUri();
            Assert.IsNotNull(resultUri);

            Assert.AreEqual("/api/I/Routes/Search", resultUri.ToString());
        }

        [TestMethod]
        public void Build_With_Simple_UriArguments()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search/{id}");

            Assert.IsNotNull(definition);
            Assert.AreEqual("Routes", definition.Controller);
            Assert.AreEqual("Search/{id}", definition.ActionWithArguments);
            Assert.AreEqual("api/I", definition.ApiVersion);

            Uri resultUri = definition.GetUri();
            Assert.IsNotNull(resultUri);

            Assert.AreEqual("/api/I/Routes/Search/{id}", resultUri.ToString());
        }

        [TestMethod]
        public void Build_With_GET_Argument()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Users", "Search");

            SearchCriteria searchObject = new SearchCriteria();
            searchObject.Page = 1;
            searchObject.PageSize = 10;
            searchObject.SubObject = new CriteriaDef
            {
                Value = "1-ABC-123",
                Condition = "StartsWith"
            };

            Assert.IsNotNull(definition);
            Assert.AreEqual("Users", definition.Controller);
            Assert.AreEqual("Search", definition.ActionWithArguments);
            Assert.AreEqual("api/I", definition.ApiVersion);

            Uri resultUri = definition.GetUri("model", searchObject);
            Assert.IsNotNull(resultUri);

            Assert.AreEqual(
                "/api/I/Users/Search?model.page=1&model.pageSize=10&model.subObject.value=1-ABC-123&model.subObject.condition=StartsWith",
                resultUri.ToString());
        }


        [TestMethod]
        public void EndpointDefinition_GET_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = definition
                .Get()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());
        }

        [TestMethod]
        public void EndpointDefinition_GET_No_Arguments_HandlesCancel()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;
            bool cancelHandlerCalled = false;

            var result = definition
                .Get()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .OnTimeout(() => { cancelHandlerCalled = true; })
                .ExecuteAsync(10).Result;

            Assert.AreEqual(false, errorHandlerCalled);
            Assert.AreEqual(true, cancelHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void EndpointDefinition_DenyDoubleRegistration_Timeout()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            var result = definition
                .Get()
                .OnTimeout(() => {  })
                .OnTimeout(() => {  })
                .ExecuteAsync(10).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void EndpointDefinition_DenyDoubleRegistration_Error()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            var result = definition
                .Get()
                .OnError((c) => { })
                .OnError((c) => { })
                .ExecuteAsync(10).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void EndpointDefinition_DenyDoubleRegistration_Success()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            var result = definition
                .Get()
                .OnSuccess((c) => { })
                .OnSuccess((c) => { })
                .ExecuteAsync(10).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EndpointDefinition_GET_No_Arguments_Timeout_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = definition
                .Get()
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync(10000).Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());
        }

        [TestMethod]
        public void EndpointDefinition_GET_No_Arguments_DefineBehavior()
        {
            Mock<IRestBehavior> restBehavior = new Mock<IRestBehavior>();

            var definition = EndpointDefinition.Build(_baseUri, "Routes", "Search");

            bool errorHandlerCalled = false;

            var result = definition
                .Get()
                .Behavior(restBehavior.Object)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync(1000).Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());

            restBehavior.Verify(b => b.OnRequestCreated(It.IsAny<HttpRequestMessage>()), Times.Once);
        }

        [TestMethod]
        public void EndpointDefinition_POST_No_Arguments_HandlesError()
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

            var result = definition
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

        [TestMethod]
        public void EndpointDefinition_PUT_No_Arguments_HandlesError()
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

            var result = definition
                .Put(searchObject)
                .OnError(httpCode => { errorHandlerCalled = true; })
                .ExecuteAsync().Result;

            Assert.AreEqual(true, errorHandlerCalled);
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.IsSucceeded);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(new Uri(_baseUri, "/api/I/Routes/Search"), result.Uri.ToString());
        }

        [TestMethod]
        public void EndpointDefinition_DELETE_No_Arguments_HandlesError()
        {
            var definition = EndpointDefinition.Build(_baseUri, "User");

            bool errorHandlerCalled = false;

            var result = definition
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
    }
}
