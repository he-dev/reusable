using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Diagnostics;
using Reusable.Utilities.MSTest.Mocks;

namespace Reusable.Tests
{
    [TestClass]
    public class AsyncDetectorTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var debugStopwatch = new StopwatchMock(new []
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(6),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(8),
            });

            var asyncDetector = new AsyncDetector(debugStopwatch);
            using (asyncDetector.BeignScope())
            {
                using (asyncDetector.BeignScope())
                {
                    using (asyncDetector.BeignScope())
                    {

                    }

                    using (asyncDetector.BeignScope())
                    {

                    }
                }
            }

            Assert.AreEqual(4, asyncDetector.MaxAsyncDegree);
            Assert.AreEqual(1, asyncDetector.AsyncGroupCount);
        }
    }
}
