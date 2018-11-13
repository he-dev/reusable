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

            Assert.AreEqual("Greeting", MemoryRx.ElementAt(0)["Identifier"]);            
            Assert.AreEqual("\"Hallo!\"", MemoryRx.ElementAt(0)["Snapshot"]);            
        }
    }
}