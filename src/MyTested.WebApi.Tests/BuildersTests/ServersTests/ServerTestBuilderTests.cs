﻿// MyTested.WebApi - ASP.NET Web API Fluent Testing Framework
// Copyright (C) 2015 Ivaylo Kenov.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
namespace MyTested.WebApi.Tests.BuildersTests.ServersTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Common.Servers;
    using NUnit.Framework;
    using Setups;
    using Setups.Models;

    [TestFixture]
    public class ServerTestBuilderTests
    {
        [Test]
        public void OwinTestsShouldWorkCorrectlyWithGlobalTestServer()
        {
            MyWebApi.Server().Starts<CustomStartup>();

            var request = new HttpRequestMessage(HttpMethod.Post, "/test");
            var jsonRequest = new HttpRequestMessage(HttpMethod.Post, "/json");
            var noModelRequest = new HttpRequestMessage(HttpMethod.Post, "/nomodel");

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(request)
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.Found);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(req => req.WithHeader("CustomHeader", "CustomValue"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(jsonRequest)
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModel(new ResponseModel { IntegerValue = 1, StringValue = "Test" });

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(noModelRequest)
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModel(new { id = 1 });

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(new HttpRequestMessage())
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.NotFound);

            MyWebApi.Server().Stops();
        }

        [Test]
        public void HttpTestsShouldWorkCorrectlyWithGlobalTestServer()
        {
            MyWebApi.Server().Starts();

            var request = new HttpRequestMessage(HttpMethod.Post, "/test");

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(request)
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.NotFound);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(req => req.WithMethod(HttpMethod.Post).WithRequestUri("api/NoAttributes/WithParameter/5"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModelOfType<int>()
                .Passing(m => m == 5);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(new HttpRequestMessage(HttpMethod.Post, "/Invalid"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.NotFound);

            MyWebApi.Server().Stops();
        }

        [Test]
        public void HttpServerTestShouldWorkCorrectlyWhenNoGlobalServerIsRunning()
        {
            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(
                    req => req.WithMethod(HttpMethod.Post).WithRequestUri("api/NoAttributes/WithParameter/5"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModelOfType<int>();

            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);
        }

        [Test]
        public void WithCustomConfigurationShouldStartHttpServerCorrectly()
        {
            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);

            MyWebApi
                .Server()
                .Working(new HttpConfiguration())
                .WithHttpRequestMessage(
                    req => req.WithMethod(HttpMethod.Post).WithRequestUri("api/NoAttributes/WithParameter/5"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.NotFound);

            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);
        }

        [Test]
        public void WithIndividualOwinHostShouldWorkCorrectly()
        {
            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);

            MyWebApi
                .Server()
                .Working<CustomStartup>()
                .WithHttpRequestMessage(req => req.WithHeader("CustomHeader", "CustomValue"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK);

            // Running second time to make sure previous server is disposed
            MyWebApi
                .Server()
                .Working<CustomStartup>()
                .WithHttpRequestMessage(req => req.WithHeader("CustomHeader", "CustomValue"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK);

            Assert.IsFalse(OwinTestServer.GlobalIsStarted);
            Assert.IsFalse(HttpTestServer.GlobalIsStarted);
        }

        [Test]
        [ExpectedException(
            typeof(InvalidOperationException),
            ExpectedMessage = "No test servers are started or could be started for this particular test case. Either call MyWebApi.Server.Starts() to start a new test server or provide global or test specific HttpConfiguration.")]
        public void WithoutAnyConfigurationServersShouldThrowException()
        {
            MyWebApi.IsUsing(null);

            MyWebApi
                .Server()
                .Working()
                .WithHttpRequestMessage(req => req.WithMethod(HttpMethod.Post).WithRequestUri("api/NoAttributes/WithParameter/5"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModelOfType<int>();
        }

        [Test]
        public void ServerStartsShouldReturnCorrectTestBuilder()
        {
            MyWebApi.IsUsing(TestObjectFactory.GetHttpConfigurationWithRoutes());

            var server = MyWebApi.Server().Starts();

            server
                .WithHttpRequestMessage(req => req.WithMethod(HttpMethod.Post).WithRequestUri("api/NoAttributes/WithParameter/5"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK)
                .AndAlso()
                .WithResponseModelOfType<int>()
                .Passing(m => m == 5);

            MyWebApi.Server().Stops();
        }

        [Test]
        public void OwinServerStartsShouldReturnCorrectTestBuilder()
        {
            MyWebApi.IsUsing(TestObjectFactory.GetHttpConfigurationWithRoutes());

            var server = MyWebApi.Server().Starts<CustomStartup>();

            server
                .WithHttpRequestMessage(req => req.WithHeader("CustomHeader", "CustomValue"))
                .ShouldReturnHttpResponseMessage()
                .WithStatusCode(HttpStatusCode.OK);

            MyWebApi.Server().Stops();
        }

        [TestFixtureTearDown]
        public void RestoreConfiguration()
        {
            MyWebApi.IsUsing(TestObjectFactory.GetHttpConfigurationWithRoutes());
        }
    }
}
