using System;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable;

namespace Reusable.Tests
{
    [TestClass]
    public class ResultTest
    {
        [TestMethod]
        public void AsEnumerable_Collection_Casted()
        {
            var nums = new[] { 1, 2, 3 };
            var result = Result<int[]>.Ok(nums);
            Assert.IsTrue(result);
            var ints = result.AsEnumerable<int>().ToList();
            CollectionAssert.AreEqual(nums, ints);
        }

        [TestMethod]
        public void implicit_operator_bool()
        {
            var result = Result<bool>.Ok(false);
            Assert.IsFalse(result);
            Assert.IsFalse(result.Value);
            Assert.IsTrue(result.Succees);
            Assert.IsFalse(result.Failure);
        }

        [TestMethod]
        public void implicit_operator_Exception_String_TimeSpan()
        {
            var result = ImplicitFail(new Exception("foo"), "bar", TimeSpan.FromSeconds(7));
            Assert.IsFalse(result);
            var (exception, message, elapsed) = result;
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", message);
            Assert.AreEqual(elapsed, TimeSpan.FromSeconds(7));
        }

        [TestMethod]
        public void implicit_operator_Exception_String()
        {
            var result = ImplicitFail(new Exception("foo"), "bar");
            Assert.IsFalse((bool)result);
            var (exception, message, elapsed) = result;
            Assert.AreEqual("foo", exception.Message);
            Assert.AreEqual("bar", message);
            Assert.AreEqual(elapsed, TimeSpan.Zero);
        }

        [TestMethod]
        public void implicit_operator_Exception_TimeSpan()
        {
            var result = ImplicitFail(new Exception("foo"), TimeSpan.FromSeconds(7));
            Assert.IsFalse((bool)result);
            var (exception, message, elapsed) = result;
            Assert.AreEqual("foo", exception.Message);
            Assert.IsNull(message);
            Assert.AreEqual(elapsed, TimeSpan.FromSeconds(7));
        }

        [TestMethod]
        public void implicit_operator_Exception()
        {
            var result = ImplicitFail(new Exception("foo"));
            Assert.IsFalse((bool)result);
            var (exception, message, elapsed) = result;
            Assert.AreEqual("foo", exception.Message);
            Assert.IsNull(message);
            Assert.AreEqual(elapsed, TimeSpan.Zero);
        }

        private static Result<string> ImplicitFail(Exception exception, string message, TimeSpan elapsed)
        {
            return (exception, message, elapsed);
        }

        private static Result<string> ImplicitFail(Exception exception, string message)
        {
            return (exception, message);
        }

        private static Result<string> ImplicitFail(Exception exception, TimeSpan elapsed)
        {
            return (exception, elapsed);
        }

        private static Result<string> ImplicitFail(Exception exception)
        {
            return exception;
        }
    }

    [TestClass]
    public class TryTest
    {
        [TestMethod]
        public void Execute_DivisionByZero_Failure()
        {
            var x = 0;
            var result = Try.Execute(() => 4 / x);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Execute_DivisionByNonZero_Success()
        {
            var result = Try.Execute(() => 4 / 2);
            Assert.IsTrue(result);
            Assert.AreEqual(2, (int)result);
        }
    }
}
