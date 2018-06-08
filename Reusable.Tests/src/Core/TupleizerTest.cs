using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class TupleizerTest
    {
        [TestMethod]
        public void Parse_TwoValues_TwoResults()
        {
            var (success, (name, count)) = "foo3".Parse<string, int?>(@"(?<T1>(?i:[a-z]+))(?<T2>\d+)?");

            Assert.IsTrue(success);
            Assert.AreEqual("foo", name);
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void Parse_OneValue_OneResult()
        {
            var (success, (name, count)) = "foo".Parse<string, int?>(@"(?<T1>(?i:[a-z]+))(?<T2>\d+)?");

            Assert.IsTrue(success);
            Assert.AreEqual("foo", name);
            Assert.IsNull(count);
        }

        [TestMethod]
        public void Parse_NoValues_NoResults()
        {
            var (success, (name, count)) = "-".Parse<string, int?>(@"(?<T1>(?i:[a-z]+))(?<T2>\d+)?");

            Assert.IsFalse(success);
            Assert.IsNull(name);
            Assert.IsNull(count);
        }
    }
}
