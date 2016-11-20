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
    public class RetryTest
    {
        [TestMethod]
        public void Execute_AllAttemptsFail()
        {
            var retry = new Retry(new RegularSequence<TimeSpan>(value: TimeSpan.FromSeconds(0.5), count: 6));
            var failingAction = new Action(() =>
            {
                throw new Exception("Test action failed.");
            });

            retry.Execute(failingAction, attempt => { attempt.Handled = true; });
            retry.Count.Verify().IsEqual(6);
            retry.Exceptions.Count().Verify().IsEqual(6);
            retry.Success.Verify().IsFalse();
        }

        [TestMethod]
        public void Execute_LastAttemptSucceeds()
        {
            var retry = new Retry(new RegularSequence<TimeSpan>(value: TimeSpan.FromSeconds(0.5), count: 6))
            {
                Log = message => Debug.WriteLine(message)
            };
            var counter = 0;
            var failingAction = new Action(() =>
            {
                if (counter++ < 5)
                {
                    throw new Exception("Test action failed.");
                }
            });

            retry.Execute(failingAction, attempt => { attempt.Handled = true; });
            retry.Count.Verify().IsEqual(6);
            retry.Exceptions.Count().Verify().IsEqual(5);
            retry.Success.Verify().IsTrue();
        }
    }
}
