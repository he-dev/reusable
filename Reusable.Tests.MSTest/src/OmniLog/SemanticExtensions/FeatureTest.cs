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
namespace Reusable.OmniLog.SemanticExtensions.Tests
{
    using static Assert;

    [TestClass]
    public class FeatureTest
    {
        
        private LoggerFactory LoggerFactory { get; set; }

        private MemoryRx MemoryRx => LoggerFactory.Observers.OfType<MemoryRx>().Single();

        private ILogger Logger => LoggerFactory.CreateLogger<FeatureTest>();

        [TestInitialize]
        public void TestInitialize()
        {
            LoggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Test")
                    .AttachObject("Product", "Reusable.Tests")
                    .AttachScope()
                    .AttachSnapshot()
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AttachElapsedMilliseconds()
                    .AddObserver(MemoryRx.Create());
        }

        [TestMethod]
        public void Log_Message_Logged()
        {
            Logger.Log(Abstraction.Layer.Infrastructure().Meta(new { Greeting = "Hallo!" }));

            IsTrue(MemoryRx.All(log => log["Name"].ToString() == "FeatureTest"), "Not all logs have the same logger");
            IsTrue(MemoryRx.All(log => log["Environment"].ToString() == "Test"), "Not all logs have the same environment");
            IsTrue(MemoryRx.All(log => log["Product"].ToString() == "Reusable.Tests"), "Not all logs have the same product");

            AreEqual("Infrastructure", MemoryRx[0].Layer());
            AreEqual(LogLevel.Debug, MemoryRx[0].Level());
            AreEqual("Meta", MemoryRx[0].Category());
            AreEqual("Greeting", MemoryRx[0].Identifier());
            AreEqual("\"Hallo!\"", MemoryRx[0].Snapshot());
            IsNull(MemoryRx[0].Elapsed());
            IsNull(MemoryRx[0].Message());
            IsNull(MemoryRx[0].Exception());
        }
    }

    internal static class LogExtensions
    {
        public static object Logger(this Log log) => log.Property<object>();
        public static object Scope(this Log log) => log.Property<object>();
        public static object Layer(this Log log) => log.Property<object>();
        public static object Level(this Log log) => log.Property<object>();
        public static object Category(this Log log) => log.Property<object>();
        public static object Identifier(this Log log) => log.Property<object>();
        public static object Snapshot(this Log log) => log.Property<object>();
        public static object Elapsed(this Log log) => log.Property<object>();
        public static object Message(this Log log) => log.Property<object>();
        public static object Exception(this Log log) => log.Property<object>();
        public static object CallerMemberName(this Log log) => log.Property<object>();
        public static object CallerLineNumber(this Log log) => log.Property<object>();
        public static object CallerFilePath(this Log log) => log.Property<object>();
    }
}