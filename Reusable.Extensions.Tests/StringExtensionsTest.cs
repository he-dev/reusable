using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Extensions.Tests
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void GetConnectionStringName_ValueWithName()
        {
            "name=Foo.bar".GetConnectionStringName().Verify().IsNotNullOrEmpty().IsEqual("Foo.bar");
        }

        [TestMethod]
        public void GetConnectionStringName_ValueWithoutName()
        {
            "Foo.bar".GetConnectionStringName().Verify().IsNullOrEmpty();
        }
    }

    [TestClass]
    public class Format
    {
        [TestMethod]
        public void ReplacesName()
        {
            Assert.AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {Color} fox {Verb} over the lazy dog.".Format(new { Color = "brown", Verb = "jumps" }),
                "Couldn't replace all letter names.");

            Assert.AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {C} fox {V} over the lazy dog.".Format(new { C = "brown", V = "jumps" }),
                "Couldn't replace single letter names");

            Assert.AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {C1b} fox {V_9o} over the lazy dog.".Format(new { C1b = "brown", V_9o = "jumps" }),
                "Couldn't replace names with underscore and digits.");

            Assert.AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {_C1b} fox {V_9o} over the lazy dog.".Format(new { _C1b = "brown", V_9o = "jumps" }),
                "Couldn't replace names beginning with an underscore.");
        }

        [TestMethod]
        public void ReplacesEscapedName()
        {
            Assert.AreEqual(
                "The quick {brown} fox jumps over the lazy dog.",
                "The quick {{brown}} fox jumps over the lazy dog.".Format(new { fox = "dummy" }));
        }

        [TestMethod]
        public void IgnoresInvalidBracePair()
        {
            Assert.AreEqual(
                "The quick {brown}} fox jumps over {{the} lazy dog.",
                "The quick {brown}} fox jumps over {{the} lazy dog.".Format(new { }));
        }

        [TestMethod]
        public void IgnoresNullValue()
        {
            Assert.AreEqual(
                "The quick {brown}} fox {Verb} over {{the} lazy dog.",
                "The quick {brown}} fox {Verb} over {{the} lazy dog.".Format(new { Verb = (string)null }));
        }

        [TestMethod]
        public void IgnoresNullObject()
        {
            Assert.AreEqual(
                "The quick {brown}} fox {Verb} over {{the} lazy dog.",
                "The quick {brown}} fox {Verb} over {{the} lazy dog.".Format((object)null));
        }

        [TestMethod]
        public void IgnoresNullString()
        {
            Assert.IsNull(((string)null).Format((object)null));
        }
    }

    [TestClass]
    public class ToJson_Exception
    {
        [TestMethod]
        public void CreateJsonString()
        {
            try
            {
                throw new FileNotFoundException("Bang!", @"C:\foo.txt");
            }
            catch (Exception ex)
            {
                //var json = ex.ToJson();
            }
        }
    }
}