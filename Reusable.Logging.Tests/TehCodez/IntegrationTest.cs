using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Loggex.Recorders;
using Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez;

namespace Reusable.Loggex.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void Log_LogLevelGreaterOrEqual_Logged()
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
            Assert.AreEqual("TestLogger", memoryRecorder.Logs.Single().Name());
        }
    }
}
