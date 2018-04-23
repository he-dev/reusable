using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Utilities.MSTest.Mocks;

namespace Reusable.Utilities.MSTest.Tests.Mocks
{
    [TestClass]
    public class MockStopwatchTest
    {
        private IStopwatch _stopwatch;

        [TestInitialize]
        public void TestInitialize()
        {
            _stopwatch = MockStopwatch.StartNew(Enumerable.Range(1, 10).Select(i => TimeSpan.FromSeconds(i)));
        }

        [TestMethod]
        public void Elapsed_GetNextValue()
        {
            _stopwatch = MockStopwatch.StartNew(Enumerable.Range(1, 10).Select(i => TimeSpan.FromSeconds(i)));

            Assert.IsTrue(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(0), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _stopwatch.Elapsed);
        }

        [TestMethod]
        public void Stop_Elapsed_GetsSameValue()
        {
            _stopwatch = MockStopwatch.StartNew(Enumerable.Range(1, 10).Select(i => TimeSpan.FromSeconds(i)));

            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Stop();

            Assert.IsFalse(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _stopwatch.Elapsed);
        }

        [TestMethod]
        public void ResetStopped_Elapsed_GetsOnlyFirst()
        {
            _stopwatch = MockStopwatch.StartNew(Enumerable.Range(1, 10).Select(i => TimeSpan.FromSeconds(i)));

            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Stop();

            _stopwatch.Reset();            

            Assert.IsFalse(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(0), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(0), _stopwatch.Elapsed);
        }

        [TestMethod]
        public void ResetRunning_Elapsed_GetsValueFromFirst()
        {
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();

            _stopwatch.Reset();

            Assert.IsTrue(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(3), _stopwatch.Elapsed);
        }

        [TestMethod]
        public void RestartRunning_Elapsed_GetsValueFromFirst()
        {
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();

            _stopwatch.Restart();

            Assert.IsTrue(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(1), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(2), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(3), _stopwatch.Elapsed);
        }

        [TestMethod]
        public void Start_AlreadyStarted_DoesNothing()
        {
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Elapsed.Noop();
            _stopwatch.Start();
            _stopwatch.Elapsed.Noop();


            Assert.IsTrue(_stopwatch.IsRunning);
            Assert.AreEqual(TimeSpan.FromSeconds(4), _stopwatch.Elapsed);
            Assert.AreEqual(TimeSpan.FromSeconds(5), _stopwatch.Elapsed);
        }
    }
}
