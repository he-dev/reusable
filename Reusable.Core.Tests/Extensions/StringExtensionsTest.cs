using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Fuse.Testing;
using Reusable.Fuse;

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

        [TestMethod]
        public void ExtractConnectionStringName_ValueWithName()
        {
            "name=Foo.bar".ExtractConnectionStringName().Verify().IsNotNullOrEmpty().IsEqual("Foo.bar");
        }

        [TestMethod]
        public void ExtractConnectionStringName_ValueWithoutName()
        {
            "Foo.bar".ExtractConnectionStringName().Verify().IsNullOrEmpty();
        }
    }
}
