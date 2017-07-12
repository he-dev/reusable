using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Logging.Loggex.Recorders;
using Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez;

namespace Reusable.Logging.Loggex.Tests
{
    [TestClass]
    public class LoggexIntegrationTest
    {
        [TestMethod]
        public void Log_DefaultLogLevel_Logged()
        {
            var memoryRecorder = new MemoryRecorder();
            var logger = Logger.Create("TestLogger", new LoggerConfiguration
            {
                Recorders = { memoryRecorder },
                Filters = { new LogFilter { Recorders = { "MemoryRecorder" } } }
            });

            logger.Log(e => e.Message("foo"));

            Assert.That.IsNotEmpty(memoryRecorder.Logs);
            Assert.That.CountEquals(1, memoryRecorder.Logs);
            Assert.AreEqual("foo", memoryRecorder.Logs.Single().Message());
            Assert.AreEqual(LogLevel.Info, memoryRecorder.Logs.Single().LogLevel());
            Assert.AreEqual("TestLogger", memoryRecorder.Logs.Single().Name());
        }

        [TestMethod]
        public void Log_LogLevelGreaterThenDefault_Logged()
        {
            var memoryRecorder = new MemoryRecorder();
            var logger = Logger.Create("TestLogger", new LoggerConfiguration
            {
                Recorders = { memoryRecorder },
                Filters = { new LogFilter { Recorders = { "MemoryRecorder" } } }
            });

            logger.Log(e => e.Message("foo").Warn());

            Assert.That.IsNotEmpty(memoryRecorder.Logs);
            Assert.That.CountEquals(1, memoryRecorder.Logs);
            Assert.AreEqual("foo", memoryRecorder.Logs.Single().Message());
            Assert.AreEqual(LogLevel.Warn, memoryRecorder.Logs.Single().LogLevel());
            Assert.AreEqual("TestLogger", memoryRecorder.Logs.Single().Name());
        }

        [TestMethod]
        public void Log_LogLevelLessThenDefault_NotLogged()
        {
            var memoryRecorder = new MemoryRecorder();
            var logger = Logger.Create("TestLogger", new LoggerConfiguration
            {
                Recorders = { memoryRecorder },
                Filters = { new LogFilter { Recorders = { "MemoryRecorder" } } }
            });

            logger.Log(e => e.Message("foo").Debug());

            Assert.That.IsEmpty(memoryRecorder.Logs);
            //Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage("Test");
        }
    }
}
