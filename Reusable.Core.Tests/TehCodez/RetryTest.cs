using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class RetryTest
    {
        [TestMethod]
        public void ExecuteAsync_Failes2of3Times_Success()
        {
            var retry = new Retry(Enumerable.Repeat(TimeSpan.FromMilliseconds(10), 3));
            var failureCount = 0;
            var action = new Action(() =>
            {
                if (failureCount < 2)
                {
                    failureCount++;
                    throw new Exception();
                }
            });
            
            retry.Execute(action);

            Assert.AreEqual(2, failureCount);
        }

        [TestMethod]
        public void ExecuteAsync_Failes3of3Times_Failure()
        {
            var retry = new Retry(Enumerable.Repeat(TimeSpan.FromMilliseconds(10), 3));
            var failureCount = 0;
            var action = new Action(() =>
            {
                if (failureCount < 3)
                {
                    failureCount++;
                    throw new Exception();
                }
            });

            Assert.ThrowsException<AggregateException>(() => retry.Execute(action));
        }
    }
}
