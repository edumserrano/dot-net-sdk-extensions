using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.Demos.Configuration
{
    // For more information on why this custom WebApplicationFactory<T> is configured as below
    // please see the doc at /docs/integration-tests/web-application-factory.md 
    public class ConfiguringWebHostWebApplicationFactory : WebApplicationFactory<StartupConfiguringWebHost>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseContentRoot(".")
                .UseStartup<StartupConfiguringWebHost>();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {

                });
        }
    }
}