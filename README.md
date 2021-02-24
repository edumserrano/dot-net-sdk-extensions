# Extensions and guides for .NET SDK

This repo contains extensions to use with .NET applications, using .net core 3.1 and higher, as well as extensions for unit and [integration tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?#introduction-to-integration-tests).

It also contains guides on scenarios around building apps using .NET SDK. These guides are for situations where an extension is not merited but some documentation on how to accomplish the task is.

## Documentation and Guides

For documentation about the available extensions go [here](/docs/docs-main.md).

For the .NET SDK guides available go [here](/docs/guides-main.md).

## GitHub Workflows

| Worflow                   |      Status and link      |
|---------------------------|:-------------------------:|
| [build-demos](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/build-demos.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Build%20demos/badge.svg) |
| [nuget-publish](https://github.com/edumserrano/dot-net-sdk-extensions/blob/master/.github/workflows/nuget-publish.yml)             |  ![Build Status](https://github.com/edumserrano/dot-net-sdk-extensions/workflows/Publish%20Nuget%20packages/badge.svg) |

For more information about the GitHub actions go [here](/docs/github-workflows/github-workflows.md).

## Installing

This repo provides two NuGet packages:

- [DotNet-Sdk-Extensions](https://www.nuget.org/packages/DotNet-Sdk-Extensions)
- [DotNet-Sdk-Extensions-Testing](https://www.nuget.org/packages/DotNet-Sdk-Extensions-Testing)

Installation is performed via NuGet and you can do it using the following commands:

```
dotnet add package DotNet-Sdk-Extensions
dotnet add package DotNet-Sdk-Extensions-Testing
```

## Debugging

The NuGet packages published include symbols generated with [sourcelink](https://github.com/dotnet/sourcelink).

For more information on how to debug the NuGet packages code from your application see:

- [Exploring .NET Core's SourceLink - Stepping into the Source Code of NuGet packages you don't own](https://www.hanselman.com/blog/exploring-net-cores-sourcelink-stepping-into-the-source-code-of-nuget-packages-you-dont-own)
- [How to Configure Visual Studio to Use SourceLink to Step into NuGet Package Source](https://aaronstannard.com/visual-studio-sourcelink-setup/)
- [Source Link - microsoft docs](https://docs.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)

## Building

### Using Visual Studio

1) Clone the repo and open one of the solution files:
   - **DotNet.Sdk.Extensions.sln:** source for the extensions.
   - **DotNet.Sdk.Extensions.Demos.sln:** demo projects for the extensions and the guides.

2) Press build on Visual Studio.

### Using dotnet CLI

1) Clone the repo and browse to the directory using your favorite shell.

2) Run:
   - **`dotnet build DotNet.Sdk.Extensions.sln`:** to build the source for the extensions.
   - **`dotnet build DotNet.Sdk.Extensions.Demos.sln`:** to build the demos for the extensions and the guides.

## License

This project is licensed under the [MIT license](https://licenses.nuget.org/MIT).

## TODO


* move packages from alpha to stable and update it on demo sln
* any readme missing?
* overwrite configuration items on options without adding a whole new appsettings
* http mocking (.UseHttpMocks or MockHttpResponse methods) should allow access to the service provider
* investigate how to test using Ilogger
* investigate serilog logging test extensions for integration tests
  * investigate test sink (search github) and look at how the test is done for the enricher https://github.com/serilog/serilog-enrichers-environment/commit/24c1c874ed4044ec488a5119130c869d328f9fbd  
* explain how to set loglevels for integration tests output https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0#configure-logging
  * maybe add it as an extension and use it on where I have similar functionality (HostBuilderExtensions and HttpMockServerBuilderExtensions)
* create an issue on the repo to allow testing https. need to figure out how to configure a cert. Look for TODO on the codebase
* make sure servers are disposed on the tests and demo tests
* replace the configuration usages as below with the new extension method to inject configuration values that is also based on the below configuration
```
.ConfigureAppConfiguration((context, builder) =>
                {
                    var memoryConfigurationSource = new MemoryConfigurationSource
                    {
                        InitialData = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("SomeOption", "some value")
                            }
                    };
                    builder.Add(memoryConfigurationSource);
                })
```
* note to docs about configuration on integration tests that another way to do it should be to use:
```
 var web = _web.WithWebHostBuilder(
                builder => builder
                    .ConfigureServices(sc => sc.Configure<RequestLoggingOptions>(options =>
                    {
                        options.Logger = logger;
                        options.EnrichDiagnosticContext += (diagnosticContext, httpContext) =>
                        {
                            diagnosticContext.Set("SomeString", "string");
                        };
                    }))
```
* similar method to WebHostBuilderExtensions.AddTestAppSettings but for IHost instead of WebHost
* where to save info about how to use splunk + docker + asp.net core app (+ serilog?)
* note about using async local to implement data sharing on the context of a request execution (see my elapsedStats implementation)

## Notes

If you have problems with SSL certificates when running the demos or tests then make sure you have trusted dev certificates by executing the following command

```
dotnet dev-certs https --trust
```

For more info see [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide).
