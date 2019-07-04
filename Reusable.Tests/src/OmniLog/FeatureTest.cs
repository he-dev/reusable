using System;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Attachments;
using Xunit;

namespace Reusable.Tests.OmniLog
{
    public class FeatureTest: IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger _logger;

        private readonly MemoryRx _memoryLog;

        public FeatureTest()
        {
            _loggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Test")
                    .AttachObject("Product", "Reusable.OmniLog")
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AddObserver(_memoryLog = new MemoryRx());

            _logger = _loggerFactory.CreateLogger<FeatureTest>();
        }

        [Fact]
        public void asdf()
        {
            
        }
        
        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}