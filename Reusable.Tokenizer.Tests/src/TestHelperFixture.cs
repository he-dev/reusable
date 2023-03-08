using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Octopus;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Connectors;

namespace Reusable
{
    using static TestHelper;

    [UsedImplicitly]
    public class TestHelperFixture : IDisposable
    {
        public TestHelperFixture()
        {
            LoggerFactory = CreateLoggerFactory(new MemoryConnector());
            Cache = CreateCache();
            Resource = CreateResource();
        }

        public ILoggerFactory LoggerFactory { get; }

        public IResource Resource { get; }

        public IMemoryCache Cache { get; }

        public void Dispose()
        {
            LoggerFactory.Dispose();
            Cache.Dispose();
        }
    }
}