using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Rx;
using Reusable.Translucent;

namespace Reusable
{
    using static TestHelper;
    
    [UsedImplicitly]
    public class TestHelperFixture : IDisposable
    {
        public TestHelperFixture()
        {
            Logs = new MemoryRx();
            LoggerFactory = CreateLoggerFactory(Logs);
            Cache = CreateCache();
            
            Resources =
                ResourceRepository
                    .From<TestResourceSetup>(
                        ImmutableServiceProvider
                            .Empty
                            .Add(Cache)
                            .Add(LoggerFactory));
        }

        
        public MemoryRx Logs { get; }

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