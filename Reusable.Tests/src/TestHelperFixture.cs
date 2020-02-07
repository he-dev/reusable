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
            
            Resources =
                ResourceRepository
                    .From<TestResourceSetup>(
                        ImmutableServiceProvider
                            .Empty
                            .Add(Cache)
                            .Add(LoggerFactory));
        }

        public ILoggerFactory LoggerFactory { get; }

        public IResourceRepository Resources { get; }
        
        public IMemoryCache Cache { get; }

        public void Dispose()
        {
            LoggerFactory.Dispose();
            Cache.Dispose();
        }
    }
}