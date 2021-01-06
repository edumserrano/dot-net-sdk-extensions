﻿using System;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.WebHostBuilders;
using DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders.Auxiliar.Timeout;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.WebHostBuilders
{
    public class TimeoutTests : IClassFixture<TimeoutHttpResponseMockingWebApplicationFactory>
    {
        private readonly TimeoutHttpResponseMockingWebApplicationFactory _webApplicationFactory;

        public TimeoutTests(TimeoutHttpResponseMockingWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        /// <summary>
        /// The setup for this sets up a named HttpClient "named-client" without further configuration.
        /// This tests that if we define a mock to timeout it will timeout as expected.
        /// </summary>
        [Fact]
        public async Task TimeoutOnHttpClientWithDefaultTimeout()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("named-client")
                                .TimesOut(TimeSpan.FromMilliseconds(50));
                        });
                    });
                })
                .CreateClient();

            // for some reason Should.Throw or Should.ThrowAsync weren't producing the same exception working so I
            // did the equivalent try/catch code
            Exception? expectedException = null;
            try
            {
                await httpClient.GetAsync("/named-client");
            }
            catch (Exception exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException!.GetType().ShouldBe(typeof(TaskCanceledException));
            expectedException.Message.ShouldBe("Timeout triggered after 00:00:00.0500000.");
        }

        /// <summary>
        /// The setup for this sets up a named HttpClient "named-client-with-timeout" and with a configured timeout of 50ms.
        /// This tests that if we define a mock to timeout it will timeout as expected.
        /// </summary>
        [Fact]
        public async Task TimeoutOnHttpClientWithTimeoutConfigured()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("named-client-with-timeout")
                                .TimesOut(TimeSpan.FromSeconds(80));
                        });
                    });
                })
                .CreateClient();

            // for some reason Should.Throw or Should.ThrowAsync weren't producing the same exception working so I
            // did the equivalent try/catch code
            Exception? expectedException = null;
            try
            {
                await httpClient.GetAsync("/named-client-with-timeout");
            }
            catch (Exception exception)
            {
                expectedException = exception;
            }

            expectedException.ShouldNotBeNull("Expected TaskCanceledException but didn't get any.");
            expectedException!.GetType().ShouldBe(typeof(TaskCanceledException));
            expectedException.Message.ShouldBe("The request was canceled due to the configured HttpClient.Timeout of 0.05 seconds elapsing.");
        }

        /// <summary>
        /// The setup for this test uses Polly to define a timeout policy for the named HttpClient "polly-named-client".
        /// The timeout for the HttpClient is set to 50ms and the HttpClient is invoked when doing a GET to /polly-named-client.
        /// This tests that if we define a mock to timeout it will timeout as expected.
        /// Polly throws a TimeoutRejectedException when a timeout occurs.
        /// </summary>
        [Fact]
        public async Task TimeoutWithPolly()
        {
            var httpClient = _webApplicationFactory
                .WithWebHostBuilder(webHostBuilder =>
                {
                    webHostBuilder.UseHttpMocks(handlers =>
                    {
                        handlers.MockHttpResponse(httpResponseMessageBuilder =>
                        {
                            httpResponseMessageBuilder
                                .ForNamedClient("polly-named-client")
                                .TimesOut(TimeSpan.FromMilliseconds(80));
                        });
                    });
                })
                .CreateClient();

            var exception = await Should.ThrowAsync<TimeoutRejectedException>(httpClient.GetAsync("/polly-named-client"));
            exception.Message.ShouldBe("The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.");
        }
    }
}
