using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable;

namespace DotNetBits.CronExpressionBit.Tests
{
    [TestClass]
    public class CronExpressionTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var ce = CronExpression.Parse("0 10-20/5 *");

            var match1 = ce.IsMatch(new DateTime(2016, 10, 3, 8, 15, 0));
            var match2 = ce.IsMatch(new DateTime(2016, 10, 3, 8, 16, 0));

            Assert.IsTrue(match1);
            Assert.IsFalse(match2);
        }
    }
}
