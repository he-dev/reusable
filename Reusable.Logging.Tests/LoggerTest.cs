using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Logging.Loggex.Tests.Mocks;

namespace Reusable.Logging.Loggex.Tests
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
