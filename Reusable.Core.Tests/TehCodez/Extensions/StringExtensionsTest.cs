using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void NonEmptyOrNull_Empty_Null()
        {
            Assert.IsNull(string.Empty.NullIfEmpty());
        }

        [TestMethod]
        public void NonEmptyOrNull_Null_Null()
        {
            Assert.IsNull(((string)null).NullIfEmpty());
        }

        [TestMethod]
        public void NonEmptyOrNull_NonEmpty_NonEmpty()
        {
            Assert.AreEqual("foo", "foo".NullIfEmpty());
        }


        [TestMethod]
        public void NonWhitespaceOrNull_Empty_Null()
        {
            Assert.IsNull(string.Empty.NullIfWhitespace());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_Null_Null()
        {
            Assert.IsNull(((string)null).NullIfWhitespace());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_Whitespace_Null()
        {
            Assert.IsNull(((string)"    ").NullIfWhitespace());
        }

        [TestMethod]
        public void NonWhitespaceOrNull_NonEmpty_NonEmpty()
        {
            Assert.AreEqual("foo", "foo".NullIfWhitespace());
        }       
    }
}
