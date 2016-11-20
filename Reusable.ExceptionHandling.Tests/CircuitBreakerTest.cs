using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.ExceptionHandling.Tests
{
    [TestClass]
    public class CircuitBreakerTest
    {
        [TestMethod]
        public void TestEverything()
        {
            var referenceTimestamp = new DateTime(2016, 11, 12, 9, 0, 0);

            var threshold = new Threshold(count: 3, interval: TimeSpan.FromSeconds(5));
            var fuse = new CircuitBreaker(threshold, timeout: TimeSpan.FromSeconds(10))
            {
                Clock = new TestClock
                {
                    UtcNow = referenceTimestamp
                }
            };
            fuse.Pass(2);
            fuse.Count.Verify().IsEqual(2);
            fuse.State.Verify().IsEqual(CircutBreakerState.Closed);
            fuse.TimedOut.Verify().IsFalse();
            fuse.AutoReset.Verify().IsTrue();
            fuse.StartedOn.Verify().IsNotNull().Value.Value.Verify().IsEqual(referenceTimestamp);

            fuse.PassOne();
            fuse.Count.Verify().IsEqual(3);
            fuse.State.Verify().IsEqual(CircutBreakerState.Open);
            fuse.TimedOut.Verify().IsFalse();

            (fuse.Clock as TestClock).UtcNow = referenceTimestamp.AddSeconds(15);
            fuse.TimedOut.Verify().IsTrue();
            fuse.PassOne();
            fuse.Count.Verify().IsEqual(1);
            fuse.State.Verify().IsEqual(CircutBreakerState.Open);
            fuse.TimedOut.Verify().IsFalse();

        }
    }
}
