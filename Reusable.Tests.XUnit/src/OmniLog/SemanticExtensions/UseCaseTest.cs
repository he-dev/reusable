using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Attachments;
using Xunit;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Tests.OmniLog.SemanticExtensions
{
    public class UseCaseTest2 : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger _logger;

        private readonly MemoryRx _memoryLog;

        public UseCaseTest2()
        {
            _loggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Test")
                    .AttachObject("Product", "Reusable.OmniLog")
                    .AttachScope()
                    .AttachSnapshot()
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AttachElapsedMilliseconds()
                    .AddObserver(_memoryLog = new MemoryRx());

            _logger = _loggerFactory.CreateLogger<UseCaseTest2>();

            LogScope.NewCorrelationId = DefaultCorrelationId.New;
        }

        [Fact]
        public void Logs_semantic_extensions()
        {
            using (_logger.BeginScope().WithRoutine("TestRoutine"))
            {
                _logger.Log(Abstraction.Layer.Infrastructure().Decision("Say hallo!").Because("Seen friend."));

                using (_logger.BeginScope())
                {
                    _logger.Log(Abstraction.Layer.Infrastructure().RoutineFromScope().Running());
                }

                var decisionLog = _memoryLog.Single(l => l["Category"].Equals("Flow") && l["Identifier"].Equals("Decision"));
                var routineLog = _memoryLog.Single(l => l["Category"].Equals("Routine") && l["Identifier"].Equals("TestRoutine"));

                Assert.Equal("\"Say hallo!\"", decisionLog["Snapshot"]);
                Assert.Equal("\"Running\"", routineLog["Snapshot"]);
            }
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}