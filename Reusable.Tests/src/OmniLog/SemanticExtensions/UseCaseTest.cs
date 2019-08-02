using System;
using System.Linq;
using Reusable.Extensions;
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
            _memoryLog = new MemoryRx();
            _loggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Test")
                    .AttachObject("Product", "Reusable.OmniLog")
                    .AttachScope()
                    .AttachSnapshot()
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AttachElapsedMilliseconds()
                    .Subscribe(_memoryLog, Functional.Echo);

            _logger = _loggerFactory.CreateLogger<UseCaseTest2>();

            LogScope.NewCorrelationId = LogCorrelationId.NewGuid;
        }

        [Fact]
        public void Logs_semantic_extensions()
        {
            using (_logger.BeginScope().Routine("TestRoutine"))
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