using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests
{
    using static Assert;

    [TestClass]
    public class StringInterpolationTest
    {
        [TestMethod]
        public void CanFormatNestedExpressions()
        {
            var sentence = "The quick {color} fox {verb} {{over}} {who}.";//.Format(new { Color = "brown", Verb = "jumps" }),

            var values = new Dictionary<string, object>
            {
                ["color"] = "brown",
                ["verb"] = "jumps",
                ["who"] = "the lazy {animal}",
                ["animal"] = "{firstName} {lastName}",
                ["firstName"] = "dog",
                ["lastName"] = "Sam"
            };

            AreEqual("The quick brown fox jumps {over} the lazy dog Sam.", sentence.Format(values));
        }

        [TestMethod]
        public void CanDetectRecursiveExpressions()
        {
            var sentence = "The quick {color} fox {verb} {{over}} {who}.";//.Format(new { Color = "brown", Verb = "jumps" }),

            var values = new Dictionary<string, object>
            {
                ["color"] = "brown",
                ["verb"] = "jumps",
                ["who"] = "the lazy {animal}",
                // The next two lines are recursive.
                ["animal"] = "{firstName} {lastName}",
                ["firstName"] = "{animal}",
                ["lastName"] = "Sam"
            };

            That.Throws<DynamicException>(() => sentence.Format(values));
        }

        [TestMethod]
        public void CanFormatFromAnonymousObject()
        {
            AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {Color} fox {Verb} over the lazy dog.".Format(new { Color = "brown", Verb = "jumps" }),
                "Couldn't replace all letter names.");

            AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {C} fox {V} over the lazy dog.".Format(new { C = "brown", V = "jumps" }),
                "Couldn't replace single letter names");

            AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {C1b} fox {V_9o} over the lazy dog.".Format(new { C1b = "brown", V_9o = "jumps" }),
                "Couldn't replace names with underscore and digits.");

            AreEqual(
                "The quick brown fox jumps over the lazy dog.",
                "The quick {_C1b} fox {V_9o} over the lazy dog.".Format(new { _C1b = "brown", V_9o = "jumps" }),
                "Couldn't replace names beginning with an underscore.");
        }

        [TestMethod]
        public void IgnoresExpressionWithAlignmentWhenNotFound()
        {
            AreEqual(
                "The quick {brown,4} fox jumps over the lazy dog.",
                "The quick {brown,4} fox jumps over the lazy dog.".Format(new { fox = "dummy" }));
        }

        [TestMethod]
        public void IgnoresNameWithAlignmentAndFormatWhenNotFound()
        {
            AreEqual(
                "The quick {brown,4:abc} fox jumps over the lazy dog.",
                "The quick {brown,4:abc} fox jumps over the lazy dog.".Format(new { fox = "dummy" }));
        }

        [TestMethod]
        public void CanRecognizeEscapedExpression()
        {
            AreEqual(
                "The quick {brown} fox jumps over the lazy dog.",
                "The quick {{brown}} fox jumps over the lazy dog.".Format(new { fox = "dummy" }));
        }

        [TestMethod]
        public void CanRecognizeEscapedExpressionWithAlignment()
        {
            AreEqual(
                "The quick {brown,4} fox jumps over the lazy dog.",
                "The quick {{brown,4}} fox jumps over the lazy dog.".Format(new { fox = "dummy" }));
        }

        [TestMethod]
        public void IgnoresUnbalancedBracePair()
        {
            AreEqual(
                "The quick {brown}} fox jumps over {{the} lazy dog.",
                "The quick {brown}} fox jumps over {{the} lazy dog.".Format(new { }));
        }

        [TestMethod]
        public void DoesNotCrashOnNullValue()
        {
            AreEqual(
                "The quick {brown}} fox  over {{the} lazy dog.",
                "The quick {brown}} fox {Verb} over {{the} lazy dog.".Format(new { Verb = (string)null }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DoesNotAllowNullArgs()
        {
            AreEqual(
                "The quick {brown}} fox {Verb} over {{the} lazy dog.",
                "The quick {brown}} fox {Verb} over {{the} lazy dog.".Format((object)null));
        }

        [TestMethod]
        public void IgnoresNullString()
        {
            IsNull(((string)null).Format((object)null));
        }

        [TestMethod]
        public void CanUseExpressionWithDot()
        {
            AreEqual("foo waldo qux", "foo {bar.baz} qux".Format(new Dictionary<string, object> { ["bar.baz"] = "waldo" }));
        }

        [TestMethod]
        public void ReturnsEmptyCollectionWhenNoNames()
        {
            var result = StringInterpolation.GetNames("foo bar baz").ToList();
            CollectionAssert.AreEqual(new string[] { }, result);
        }

        [TestMethod]
        public void ReturnNames()
        {
            var result = StringInterpolation.GetNames("foo {bar} baz {qux} waldo").ToList();
            CollectionAssert.AreEqual(new[] { "bar", "qux" }, result);
        }            
    }
}