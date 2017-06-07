using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Crony.Tests
{
    [TestClass]
    public class CronExpressionTest
    {
        // second minute hour    day-of-month month      day-of-week year
        // 0-59    0-59    0-23    1-31              0-11 1-7  2000-2001


        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_ExactSecond_True()
        {
            Assert.IsTrue(CronExpression.From("2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 2)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_ExactSecond_False()
        {
            Assert.IsFalse(CronExpression.From("2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 3)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_SecondRange_True()
        {
            Assert.IsTrue(CronExpression.From("2-8 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 2)));
            Assert.IsTrue(CronExpression.From("2-8 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 5)));
            Assert.IsTrue(CronExpression.From("2-8 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 8)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_SecondRange_False()
        {
            Assert.IsFalse(CronExpression.From("2-8 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 1)));
            Assert.IsFalse(CronExpression.From("2-8 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 9)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_SecondRangeWithStep_True()
        {
            Assert.IsTrue(CronExpression.From("2-8/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 2)));
            Assert.IsTrue(CronExpression.From("2-8/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 6)));
            Assert.IsTrue(CronExpression.From("2-8/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 8)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        public void Contains_SecondRangeWithStep_False()
        {
            Assert.IsFalse(CronExpression.From("2-8/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 3)));
            Assert.IsFalse(CronExpression.From("2-8/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 7)));
        }

        [TestMethod]
        [TestCategory("Seconds")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Contains_SecondOutOfRange_False()
        {
            CronExpression.From("2-88/2 * * * * *").Contains(new DateTime(2017, 5, 1, 8, 0, 3));
        }
    }
}
