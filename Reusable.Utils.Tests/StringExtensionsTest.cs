using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void NonEmptyOrNull_Empty_Null()
        {
            Assert.IsNull(string.Empty.NonEmptyOrNull());
        }

        [TestMethod]
        public void NonEmptyOrNull_Null_Null()
        {
            Assert.IsNull(((string)null).NonEmptyOrNull());
        }

        [TestMethod]
        public void NonEmptyOrNull_NonEmpty_NonEmpty()
        {
            Assert.AreEqual("foo", "foo".NonEmptyOrNull());
        }


        [TestMethod]
        public void NonWhitespaceOrNull_Empty_Null()
        {
            Assert.IsNull(string.Empty.NonWhitespaceOrNull());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_Null_Null()
        {
            Assert.IsNull(((string)null).NonWhitespaceOrNull());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_Whitespace_Null()
        {
            Assert.IsNull(((string)"    ").NonWhitespaceOrNull());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_NonEmpty_NonEmpty()
        {
            Assert.AreEqual("foo", "foo".NonWhitespaceOrNull());
        }
    }
}
