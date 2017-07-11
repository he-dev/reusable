using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Logging.Tests.Mocks;
using System.Linq;
using System.Text;
using Reusable.Loggex;

namespace Reusable.Logging.Tests
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void Log_MessageWithElapsedSeconds_ValueInserted()
        {
            var logEntry = LogEntry.Create().Message($"foo {{{nameof(MockProperty)}}} bar");
            //var logger = new MockLogger();
            //Logger.ComputedProperties.Add(new MockProperty("baz"));

            //logEntry.Log(logger);

            //Assert.AreEqual(1, logger.LogEntries.Count);
            //Assert.AreEqual("foo baz bar", logger.LogEntries.First().GetValue<StringBuilder>("message").ToString());
        }
    }
}
