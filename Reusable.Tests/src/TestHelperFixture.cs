using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Translucent;

namespace Reusable
{
    using static TestHelper;

    [UsedImplicitly]
    public class TestHelperFixture : IDisposable
    {
        public TestHelperFixture()
        {
            LoggerFactory = CreateLoggerFactory(new MemoryRx());
            Cache = CreateCache();

            Resources = new Resource
            (
                ImmutableServiceProvider.Empty.Add(Cache).Add(LoggerFactory),
                TestResourceFactory.CreateControllers,
                TestResourceFactory.CreateMiddleware
            );
            
            Resources =
                Resource
                    .Builder()
                    .UseController()
        }

        public ILoggerFactory LoggerFactory { get; }

        public IResource Resources { get; }

        public IMemoryCache Cache { get; }

        public void Dispose()
        {
            LoggerFactory.Dispose();
            Cache.Dispose();
        }
    }
}