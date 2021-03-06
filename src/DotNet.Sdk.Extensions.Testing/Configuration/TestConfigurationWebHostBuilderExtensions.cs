﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Logging;

namespace DotNet.Sdk.Extensions.Testing.Configuration
{
    /// <summary>
    /// Provides extension methods to the <see cref="IWebHostBuilder"/> related with providing test configuration values via appsettings files.
    /// </summary>
    public static partial class TestConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds a value to the <see cref="IConfiguration"/> using a <see cref="MemoryConfigurationSource"/>.
        /// Allows overwriting specific configuration values when doing tests.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> instance.</param>
        /// <param name="key">The key of the configuration value.</param>
        /// <param name="value">The value to set on the configuration.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder UseConfigurationValue(this IWebHostBuilder builder, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(key));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(value));
            }

            return builder.ConfigureAppConfiguration((context, builder) =>
            {
                var memoryConfigurationSource = new MemoryConfigurationSource
                {
                    InitialData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(key, value)
                    }
                };
                builder.Add(memoryConfigurationSource);
            });
        }

        /// <summary>
        /// Sets the default log level for the application.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> instance.</param>
        /// <param name="logLevel">The default log level.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder UseDefaultLogLevel(this IWebHostBuilder builder, LogLevel logLevel)
        {
            return builder.UseConfigurationValue("Logging:LogLevel:Default", $"{logLevel}");
        }

        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IWebHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary>
        /// <remarks>
        /// It also makes sure that the expected loading configuration behavior is preserved by having the
        /// <see cref="CommandLineConfigurationSource"/> last and the <see cref="EnvironmentVariablesConfigurationSource"/>
        /// second to last in the <see cref="IConfigurationBuilder.Sources"/>.
        /// This way it will keep the loading configuration behavior of:
        /// configuration taken from command line first, then environment variables, then appsettings files.
        /// </remarks>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">More appsettings' filenames if required.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var options = new TestConfigurationOptions();
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        /// <summary>
        /// Clears loaded appsettings files by removing all <see cref="JsonConfigurationSource"/>
        /// from the <see cref="IWebHostBuilder"/> and adding instances of <see cref="JsonConfigurationSource"/> for
        /// the provided appsettings files.
        /// </summary> 
        /// <remarks>
        /// It also makes sure that the expected loading configuration behavior is preserved by having the
        /// <see cref="CommandLineConfigurationSource"/> last and the <see cref="EnvironmentVariablesConfigurationSource"/>
        /// second to last in the <see cref="IConfigurationBuilder.Sources"/>.
        /// This way it will keep the loading configuration behavior of:
        /// configuration taken from command line first, then environment variables, then appsettings files.
        /// </remarks>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to add the appsettings file to.</param>
        /// <param name="configureOptions">Options for the test appsettings.</param>
        /// <param name="appSettingsFilename">Appsettings filename.</param>
        /// <param name="otherAppsettingsFilenames">The appsettings' filenames.</param>
        /// <returns>The <see cref="IWebHostBuilder"/> for chaining.</returns>
        public static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            Action<TestConfigurationOptions> configureOptions,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (configureOptions is null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new TestConfigurationOptions();
            configureOptions(options);
            return builder.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
        }

        private static IWebHostBuilder AddTestAppSettings(
            this IWebHostBuilder builder,
            TestConfigurationOptions options,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(appSettingsFilename)) throw new ArgumentException("Cannot be null or white space.", nameof(appSettingsFilename));
            if (otherAppsettingsFilenames is null) throw new ArgumentNullException(nameof(otherAppsettingsFilenames));
            if (otherAppsettingsFilenames.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Cannot have an element that is null or white space.", nameof(otherAppsettingsFilenames));

            return builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddTestAppSettings(options, appSettingsFilename, otherAppsettingsFilenames);
            });
        }

        private static IConfigurationBuilder AddTestAppSettings(
            this IConfigurationBuilder config,
            TestConfigurationOptions options,
            string appSettingsFilename,
            params string[] otherAppsettingsFilenames)
        {
            var projectDir = options.IsRelative
                ? Path.Combine(Directory.GetCurrentDirectory(), options.AppSettingsDir)
                : options.AppSettingsDir;

            /*
             * Remove existing json sources. Without doing this the configuration files loaded
             * normally during the app might interfere with the tests.
             */
            config.Sources
                .OfType<JsonConfigurationSource>()
                .ToList()
                .ForEach(source => config.Sources.Remove(source));
            var appsettingsFilenames = new[] { appSettingsFilename }.Concat(otherAppsettingsFilenames);
            foreach (var appSettingFilename in appsettingsFilenames)
            {
                var configPath = Path.Combine(projectDir, appSettingFilename);
                config.AddJsonFile(configPath, optional: false);
            }

            /*
             * After adding test appsettings files, those sources will be last in the configuration which means that
             * even if you have an EnvironmentVariablesConfigurationSource the test appsettings source would take precedence.
             * This changes the expected loading configuration behavior.
             * To correct this we will clear all EnvironmentVariablesConfigurationSource and CommandLineConfigurationSource
             * and add then last. This way the expected loading configuraiton behavior is preserved:
             * - configuration taken from command line first, then environment variables, then appsettings files.
             */
            var environmentVariablesConfigurationSources = config.Sources
                .OfType<EnvironmentVariablesConfigurationSource>()
                .ToList();
            var commandLineConfigurationSources = config.Sources
                .OfType<CommandLineConfigurationSource>()
                .ToList();
            environmentVariablesConfigurationSources.ForEach(source => config.Sources.Remove(source));
            commandLineConfigurationSources.ForEach(source => config.Sources.Remove(source));
            environmentVariablesConfigurationSources.ForEach(source => config.Sources.Add(source));
            commandLineConfigurationSources.ForEach(source => config.Sources.Add(source));
            return config;
        }
    }
}
