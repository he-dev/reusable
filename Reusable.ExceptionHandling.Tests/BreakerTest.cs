using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.ExceptionHandling.Tests
{
    [TestClass]
    public class BreakerTest
    {
        [TestMethod]
        public void Execute_StopsAfterFourAttempts()
        {
            var retry = new Retry(new RegularSequence<TimeSpan>(value: TimeSpan.FromSeconds(0.5), count: 6))
            {
                Log = message => Debug.WriteLine(message)
            };
            var failingAction = new Action(() =>
            {
                throw new Exception("Test action failed.");
            });

            var breaker = new CancellableRetry(retry, new CircuitBreaker(new Threshold(count: 4, interval: TimeSpan.FromSeconds(2))));
            breaker.Execute(failingAction, attempt => { attempt.Handled = true; });

            breaker.CircuitBreaker.State.Verify().IsEqual(CircutBreakerState.Open);
            breaker.CircuitBreaker.Count.Verify().IsEqual(4);
            retry.Count.Verify().IsEqual(4);
            retry.Exceptions.Count().Verify().IsEqual(4);
            retry.Success.Verify().IsFalse();
        }
    }
}
