﻿using System;
using DotNet.Sdk.Extensions.Testing.HostedServices;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices
{
    public class RunUntilOptionsTests
    {
        /// <summary>
        /// Tests the default values for <seealso cref="RunUntilOptions"/>
        /// </summary>
        [Fact]
        public void DefaultValues()
        {
            var options = new RunUntilOptions();
            options.PredicateCheckInterval.ShouldBe(TimeSpan.FromMilliseconds(5));
            options.Timeout.ShouldBe(TimeSpan.FromSeconds(5));
        }
    }
}
