using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

namespace Reusable.Tester.Mocks
{
    public static class MockLogger
    {
        public static (MemoryRx, ILoggerFactory, ILogger) Create(DateTime firstTimestamp, int seconds)
        {
            return Create(Enumerable.Range(0, seconds).Select(offset => firstTimestamp.AddSeconds(offset)));
        }

        public static (MemoryRx, ILoggerFactory, ILogger) Create(IEnumerable<DateTime> timestamps = null)
        {
            var memoryRx = MemoryRx.Create();

            var loggerFactory = new LoggerFactory
            {
                Observers = { memoryRx },
                Configuration = new LoggerConfiguration
                {
                    Attachements =
                    {
                        new MockTimestamp(timestamps)
                    }
                }
            };

            return (memoryRx, loggerFactory, loggerFactory.CreateLogger("MockLogger"));
        }
    }
}