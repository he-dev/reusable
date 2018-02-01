using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OmniLog.Collections;
using Reusable.Utilities.MSTest;
using Reusable.Utilities.MSTest.Mocks;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Tests
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void Log_Message_Logged()
        {
            var timestamp = new DateTime(2017, 5, 1);
            var (memoryRx, _, logger) = MockLogger.Create(timestamp, 10);

            logger.Information("Hallo OmniLog!");

            Assert.AreEqual(1, memoryRx.Logs.Count);

            var log = memoryRx.Logs.First();

            Assert.AreEqual("MockLogger", log.Name());
            Assert.AreEqual(LogLevel.Information, log.Level());
            Assert.AreEqual("Hallo OmniLog!", log.Message());
            Assert.AreEqual(timestamp, log.Timestamp());
        }

        [TestMethod]
        public void Log_LogFilterGreaterThenOrEqual_Logged()
        {
            var memoryRx = MemoryRx.Create();
            var loggerFactory = new LoggerFactory
            {
                Observers = { memoryRx },
                Configuration = new LoggerConfiguration
                {
                    LogPredicate = LogFilter.Any.Min(LogLevel.Information)
                }
            };

            var logger = loggerFactory.CreateLogger("TestLogger");

            logger.Trace("Hallo trace!");
            logger.Debug("Hallo debug!");
            logger.Information("Hallo information!");
            logger.Warning("Hallo warning!");
            logger.Error("Hallo error!");
            logger.Fatal("Hallo fatal!");

            Assert.AreEqual(4, memoryRx.Logs.Count);
        }

        [TestMethod]
        public void Log_LogFilterContains_Logged()
        {
            //var (memoryRx, loggerFactory, logger) = MockLogger.Create(new DateTime(2017, 5, 1), 10);

            var memoryRx = MemoryRx.Create();
            var loggerFactory = new LoggerFactory
            {
                Observers = { memoryRx },
                Configuration = new LoggerConfiguration
                {
                    LogPredicate = LogFilter.Any.Contains(LogLevel.Debug | LogLevel.Error)
                }
            };

            var logger = loggerFactory.CreateLogger("TestLogger");

            logger.Trace("Hallo trace!");
            logger.Debug("Hallo debug!");
            logger.Information("Hallo information!");
            logger.Warning("Hallo warning!");
            logger.Error("Hallo error!");
            logger.Fatal("Hallo fatal!");

            Assert.AreEqual(2, memoryRx.Logs.Count);
        }

        [TestMethod]
        public void Log_NestedScopes_ScopeNames()
        {
            var (memoryRx, _, logger) = MockLogger.Create(new DateTime(2017, 5, 1), 10);

            using (logger.BeginScope("foo", new { TransactionId = 1 }))
            using (logger.BeginScope("bar", new { TransactionId = 2 }))
            {
                logger.Debug("Hallo debug!");
                var scopes = memoryRx.Logs.Single().Scopes().ToList();
                Assert.AreEqual(2, scopes.Count);
                CollectionAssert.AreEqual(new[] { "bar", "foo" }, scopes.Select(s => s.ToString()).ToList());
            }
        }

        [TestMethod]
        public void Log_TwoRxs_Logged()
        {
            var memoryRx1 = MemoryRx.Create();
            var memoryRx2 = MemoryRx.Create();

            var loggerFactory = new LoggerFactory { Observers = { memoryRx1, memoryRx2 } };

            var logger1 = loggerFactory.CreateLogger("Logger1");
            var logger2 = loggerFactory.CreateLogger("Logger2");

            logger1.Information("Hallo1");
            logger2.Information("Hallo2");

            Assert.AreEqual(2, memoryRx1.Logs.Count);
            Assert.AreEqual(2, memoryRx2.Logs.Count);

            logger1.Dispose();

            logger1.Information("Hallo3"); // does not log anymore
            logger2.Information("Hallo4");

            Assert.AreEqual(3, memoryRx1.Logs.Count);
            Assert.AreEqual(3, memoryRx2.Logs.Count);
            Assert.That.Collection().AreEqual(new[] { "Hallo1", "Hallo2", "Hallo4" }, memoryRx1.Logs.Select(x => x.Message()));
            Assert.That.Collection().AreEqual(new[] { "Hallo1", "Hallo2", "Hallo4" }, memoryRx2.Logs.Select(x => x.Message()));
        }
    }
}