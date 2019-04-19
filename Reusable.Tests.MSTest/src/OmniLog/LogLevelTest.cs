using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.MSTest;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Tests
{
    [TestClass]
    public class LogLevelTest
    {
        [TestMethod]
        public void Parse_SingleLogLevels_LogLevel()
        {
            var logLevel = LogLevel.Parse("debug");
            Assert.AreEqual(LogLevel.Debug, logLevel);
        }

        [TestMethod]
        public void Parse_MultipleLogLevels_LogLevel()
        {
            var logLevel = LogLevel.Parse("debug,trace,error");
            Assert.IsTrue(logLevel.Contains((ILogLevel)LogLevel.Debug));
            Assert.IsTrue(logLevel.Contains((ILogLevel)LogLevel.Trace));
            Assert.IsTrue(logLevel.Contains((ILogLevel)LogLevel.Error));
            Assert.IsFalse(logLevel.Contains((ILogLevel)LogLevel.Fatal));
            Assert.IsFalse(logLevel.Contains((ILogLevel)LogLevel.Information));
        }

        [TestMethod]
        public void Equatable_EqualValues_True()
        {
            Assert.That.Equatable().EqualsMethod().IsTrue(LogLevel.Information, LogLevel.Information);
            Assert.That.Equatable().EqualsMethod().IsTrue(LogLevel.Debug, LogLevel.Debug);
        }

        [TestMethod]
        public void Equatable_NotEqualValues_False()
        {
            Assert.That.Equatable().EqualsMethod().IsFalse(LogLevel.Information, LogLevel.Debug);
            Assert.That.Equatable().EqualsMethod().IsFalse(LogLevel.Debug, LogLevel.Error);
        }

        [TestMethod]
        public void Comparable_EqualValues_Zero()
        {
            Assert.That.Comparable().CompareTo().IsEqualTo(LogLevel.Debug, LogLevel.Debug);
        }

        [TestMethod]
        public void Comparable_LessThanValues_Zero()
        {
            Assert.That.Comparable().CompareTo().IsLessThan(LogLevel.Debug, LogLevel.Information);
            Assert.That.Comparable().CompareTo().IsLessThan(LogLevel.Information, LogLevel.Fatal);
        }

        [TestMethod]
        public void Operator_LessThanOrEqualValues_True()
        {
            //Assert.That.BinaryOperator()..Comparable().CompareTo().IsLessThen(LogLevel.Debug, LogLevel.Information, LogLevel.Debug);
            //Assert.That.Comparable().CompareTo().IsLessThen(LogLevel.Information, LogLevel.Fatal, LogLevel.Information);
        }
    }
}
