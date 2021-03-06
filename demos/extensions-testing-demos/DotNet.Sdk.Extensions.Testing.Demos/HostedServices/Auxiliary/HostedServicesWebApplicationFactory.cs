using DotNet.Sdk.Extensions.Testing.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Demos.HostedServices.Auxiliary
{
    // For more information on why this custom WebApplicationFactory<T> is configured as below
    // please see the doc at /docs/integration-tests/web-application-factory.md 
    // You usually do NOT need to create a custom class that implements WebApplicationFactory
    // We require this because there are multiple Startup classes in this project
    public class HostedServicesWebApplicationFactory : WebApplicationFactory<StartupHostedService>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(".")
                .UseStartup<StartupHostedService>();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseDefaultLogLevel(LogLevel.Critical)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                });
        }
    }
}