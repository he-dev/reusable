using System;
using System.Collections.Generic;
using Reusable.OmniLog;
using Reusable.OmniLog.Attachments;
using Xunit;

namespace Reusable.Tests.XUnit.OmniLog
{
    public class UseCaseTest : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        
        private readonly ILogger _logger;

        public UseCaseTest()
        {
            _loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    new MemoryRx(),
                },
                Configuration = new LoggerFactoryConfiguration
                {
                    Attachments = new HashSet<ILogAttachment>
                    {
                        new Timestamp<DateTimeUtc>(),
                    }
                }
            };

            _logger = _loggerFactory.CreateLogger<UseCaseTest>();

            LogScope.NewCorrelationId = DefaultCorrelationId.New;
        }

        [Fact]
        public void Creates_scope_with_CorrelationId()
        {
            LogScope.NewCorrelationId = () => "blub";

            using (_logger.BeginScope(out var correlationId))
            {
                Assert.Equal("blub", correlationId);
            }
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}