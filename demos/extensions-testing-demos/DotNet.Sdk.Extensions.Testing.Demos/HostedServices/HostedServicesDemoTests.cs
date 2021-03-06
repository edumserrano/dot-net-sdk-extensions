using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.Demos.HostedServices.Auxiliary;
using DotNet.Sdk.Extensions.Testing.HostedServices;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Demos.HostedServices
{
    public class HostedServicesDemoTests : IClassFixture<HostedServicesWebApplicationFactory>, IDisposable
    {
        private readonly HostedServicesWebApplicationFactory _webApplicationFactory;

        public HostedServicesDemoTests(HostedServicesWebApplicationFactory webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        [Fact]
        public async Task HostedServicesRunUntilConditionDemoTest()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => ++callCount);

            await _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<ICalculator>(calculator);
                    });
                })
                .RunUntilAsync(() => callCount >= 3);

            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public async Task HostedServicesRunUntilTimeoutDemoTest()
        {
            var callCount = 0;
            var calculator = Substitute.For<ICalculator>();
            calculator
                .Sum(Arg.Any<int>(), Arg.Any<int>())
                .Returns(1)
                .AndDoes(info => ++callCount);

            var sw = Stopwatch.StartNew();
            await _webApplicationFactory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<ICalculator>(calculator);
                    });
                })
                .RunUntilTimeoutAsync(TimeSpan.FromSeconds(2));
            sw.Stop();

            sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(1900));
            callCount.ShouldBeGreaterThanOrEqualTo(3);
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
    }
}
