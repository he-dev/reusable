using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.TestTools.UnitTesting.Infrastructure;
// ReSharper disable InconsistentNaming

namespace Reusable.Tests
{
    [TestClass]
    public class CaseInsensitiveStringTest
    {
        [TestMethod]
        public void ToString_Value_Value()
        {
            Assert.AreEqual("foo", new CaseInsensitiveString("foo").ToString());
            Assert.AreEqual("fOo", new CaseInsensitiveString("fOo").ToString());
        }
    }

    [TestClass]
    public class CaseInsensitiveStringTest_Equatable_string : EquatableTest<string>
    {
        private static readonly IEquatable<string> Equatable = new CaseInsensitiveString("foo");

        protected override IEnumerable<(IEquatable<string> This, string Other)> GetEqualObjects()
        {
            yield return (Equatable, "foo");
            yield return (Equatable, "fOo");
        }

        protected override IEnumerable<(IEquatable<string> This, string Other)> GetNotEqualObjects()
        {
            yield return (Equatable, "bar");
        }
    }

    [TestClass]
    public class CaseInsensitiveStringTest_Comparable_string : ComparableTest<string>
    {
        private static readonly IComparable<string> Comparable = new CaseInsensitiveString("foo");

        protected override IEnumerable<(IComparable<string> This, string Other)> GetObjectsGreaterThenThis()
        {
            yield return (Comparable, "foz");
        }

        protected override IEnumerable<(IComparable<string> This, string Other)> GetEqualObjects()
        {
            yield return (Comparable, "foo");
            yield return (Comparable, "fOo");
        }

        protected override IEnumerable<(IComparable<string> This, string Other)> GetObjectLessThenThis()
        {
            yield return (Comparable, "foa");
        }
    }
}
