using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using Reusable.IO;
using Reusable.OmniLog.Attachements;
using Reusable.Utilities.MSTest;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Tests
{
    [TestClass]
    public class FeatureTest
    {
        private LoggerFactory LoggerFactory { get; set; }

        private MemoryRx MemoryRx => LoggerFactory.Observers.OfType<MemoryRx>().Single();

        private ILogger Logger => LoggerFactory.CreateLogger<FeatureTest>();

        [TestInitialize]
        public void TestInitialize()
        {
            LoggerFactory = new LoggerFactory
            {
                Observers = { MemoryRx.Create() },
                Configuration = new LoggerFactoryConfiguration
                {
                    Attachements =
                    {
                        new Timestamp(Sequence.Custom(new DateTime(2017, 5, 1), previous => previous.AddSeconds(1)))
                    }
                }
            };
        }

        [TestMethod]
        public void Log_Message_Logged()
        {
            Logger.Information("Hallo OmniLog!");

            Assert.AreEqual(1, MemoryRx.Count());

            var log = MemoryRx.Single();

            Assert.AreEqual("MockLogger", log.Name());
            Assert.AreEqual(LogLevel.Information, log.Level());
            Assert.AreEqual("Hallo OmniLog!", log.Message());
            //Assert.AreEqual(timestamp, log.Timestamp());
        }

        [TestMethod]
        public void CanFilterLogByLevel()
        {
            var resources = new EmbeddedFileProvider(typeof(FeatureTest).Assembly);
            var omniLogJson = resources.GetFileInfoAsync(@"res\OmniLog\OmniLog.json").Result;
            using (var jsonStream = omniLogJson.CreateReadStream())
            {
                var memory = new MemoryRx();

                var loggerFactory = 
                    new LoggerFactory()
                        .UseConfiguration(jsonStream)
                        .AddObserver(memory);

                var logger = loggerFactory.CreateLogger<FeatureTest>();
                logger.Log(LogLevel.Trace, log => log.Message("Test1"));
                logger.Log(LogLevel.Debug, log => log.Message("Test2"));

                Assert.AreEqual(1, memory.Count());

            }
        }

        //[TestMethod]
        //public void Log_LogFilterGreaterThenOrEqual_Logged()
        //{
        //    var memoryRx = MemoryRx.Create();
        //    var loggerFactory = new LoggerFactory
        //    {
        //        Observers = { memoryRx },
        //        Configuration = new LoggerConfiguration
        //        {
        //            LogPredicate = LogFilter.Any.Min(LogLevel.Information)
        //        }
        //    };

        //    var logger = loggerFactory.CreateLogger("TestLogger");

        //    logger.Trace("Hallo trace!");
        //    logger.Debug("Hallo debug!");
        //    logger.Information("Hallo information!");
        //    logger.Warning("Hallo warning!");
        //    logger.Error("Hallo error!");
        //    logger.Fatal("Hallo fatal!");

        //    Assert.AreEqual(4, memoryRx.Logs.Count);
        //}

        //[TestMethod]
        //public void Log_LogFilterContains_Logged()
        //{
        //    //var (memoryRx, loggerFactory, logger) = MockLogger.Create(new DateTime(2017, 5, 1), 10);

        //    var memoryRx = MemoryRx.Create();
        //    var loggerFactory = new LoggerFactory
        //    {
        //        Observers = { memoryRx },
        //        Configuration = new LoggerConfiguration
        //        {
        //            LogPredicate = LogFilter.Any.Contains(LogLevel.Debug | LogLevel.Error)
        //        }
        //    };

        //    var logger = loggerFactory.CreateLogger("TestLogger");

        //    logger.Trace("Hallo trace!");
        //    logger.Debug("Hallo debug!");
        //    logger.Information("Hallo information!");
        //    logger.Warning("Hallo warning!");
        //    logger.Error("Hallo error!");
        //    logger.Fatal("Hallo fatal!");

        //    //Assert.AreEqual(2, memoryRx.Logs.Count);
        //}

        //[TestMethod]
        //public void Log_NestedScopes_CorrelationIds()
        //{
        //    var (memoryRx, _, logger) = MockLogger.Create(new DateTime(2017, 5, 1), 10);

        //    using (logger.BeginScope().WithCorrelationId(1))
        //    using (logger.BeginScope().WithCorrelationId(2))
        //    {
        //        logger.Debug("Hallo debug!");
        //        var scopeCorrelationIds = memoryRx.Logs.Single().Scopes().Select(scope => scope.CorrelationId<int>()).ToList();
        //        Assert.AreEqual(2, scopeCorrelationIds.Count);
        //        Assert.AreEqual(0, new[] { 1, 2 }.Except(scopeCorrelationIds).Count());
        //    }
        //}

        //[TestMethod]
        //public void Log_TwoRxs_Logged()
        //{
        //    var memoryRx1 = MemoryRx.Create();
        //    var memoryRx2 = MemoryRx.Create();

        //    var loggerFactory = new LoggerFactory { Observers = { memoryRx1, memoryRx2 } };

        //    var logger1 = loggerFactory.CreateLogger("Logger1");
        //    var logger2 = loggerFactory.CreateLogger("Logger2");

        //    logger1.Information("Hallo1");
        //    logger2.Information("Hallo2");

        //    Assert.AreEqual(2, memoryRx1.Logs.Count);
        //    Assert.AreEqual(2, memoryRx2.Logs.Count);

        //    logger1.Dispose();

        //    logger1.Information("Hallo3"); // does not log anymore
        //    logger2.Information("Hallo4");

        //    Assert.AreEqual(3, memoryRx1.Logs.Count);
        //    Assert.AreEqual(3, memoryRx2.Logs.Count);
        //    Assert.That.Collection().AreEqual(new[] { "Hallo1", "Hallo2", "Hallo4" }, memoryRx1.Logs.Select(x => x.Message()));
        //    Assert.That.Collection().AreEqual(new[] { "Hallo1", "Hallo2", "Hallo4" }, memoryRx2.Logs.Select(x => x.Message()));
        //}
    }
}