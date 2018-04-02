using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

// ReSharper disable InconsistentNaming

namespace Reusable.Tests
{
    [TestClass]
    [TestCategory(nameof(SoftString))]
    public class SoftStringTest
    {
        [TestMethod]
        public void ToString_NullString_Empty()
        {
            Assert.That.Collection().IsEmpty(SoftString.Create(null).ToString());            
        }

        [TestMethod]
        public void ToString_EmptyString_Empty()
        {
            Assert.That.Collection().IsEmpty(SoftString.Create(string.Empty).ToString());
            Assert.That.Collection().IsEmpty(SoftString.Empty.ToString());
        }
        
        [TestMethod]
        public void ToString_NonEmptyString_NonEmpty()
        {
            Assert.AreEqual("foo", SoftString.Create("foo").ToString());
            Assert.AreEqual("fOo", SoftString.Create("fOo").ToString());
        }

        [TestMethod]
        public void this_IndexInRange_Char()
        {
            Assert.AreEqual('r', SoftString.Create("bar")[2]);
        }

        [TestMethod]
        public void Length_StringWithWhitespace_StringWithoutWhitespace()
        {
            Assert.AreEqual(3, SoftString.Create(" fOo ").Length);
        }

        [TestMethod]
        public void Equals_SameObjects_True()
        {
            Assert.That.Equatable().EqualsMethod().IsTrue(SoftString.Create("foo"), "foo", "fOo", "foo ", " fOo", " foo ");
        }

        [TestMethod]
        public void Equals_DifferentValues_False()
        {
            Assert.That.Equatable().EqualsMethod().IsFalse(SoftString.Create("foo"), "bar", "bar ", " bar", " bar ");
        }

        [TestMethod]
        public void opEqual_SameValues_True()
        {
            Assert.That.BinaryOperator().Equality().IsTrue(default(SoftString), default(string));
            Assert.That.BinaryOperator().Equality().IsTrue(SoftString.Create("foo"), "foo", "fOo", "foo ", " fOo", " foo ");
        }

        [TestMethod]
        public void opEqual_DifferentValues_False()
        {
            Assert.That.BinaryOperator().Equality().IsFalse(SoftString.Create("foo"), "bar", " fOob");
        }

        [TestMethod]
        public void opExplicit_SoftString_String_SoftString()
        {
            Assert.That.UnaryOperator().Convert(SoftString.Create("foo")).IsEqual("foo");
        }

        [TestMethod]
        public void opImplicit_String_SoftString_SoftString()
        {
            Assert.That.UnaryOperator().Convert("foo").IsEqual(SoftString.Create("foo"));
        }

        [TestMethod]
        public void Equatable_IsCanonical_True()
        {
            Assert.That.Equatable().IsCanonical<SoftString>(SoftString.Create("foo"));
            Assert.That.Equatable().IsCanonical<string>(SoftString.Create("foo"));
        }

        [TestMethod]
        public void Comparable_IsCanonical_True()
        {
            Assert.That.Comparable().IsCanonical(
                SoftString.Create("b"), 
                SoftString.Create("a"), 
                SoftString.Create("b"), 
                SoftString.Create("c")
            );
        }

        [TestMethod]
        public void Implements_IEnumerableOfString_True()
        {
            Assert.That.Type<SoftString>().Implements<IEnumerable<char>>();
        }

        [TestMethod]
        public void MyTestMethod()
        {
            Assert.That.Comparable().CompareTo().IsLessThen(SoftString.Create("fao"), "foz", "foz ", " foz", " foz ");
            Assert.That.Comparable().CompareTo().IsEqualTo(SoftString.Create("foo"), "foo", "fOo", "fOo ", " fOo", " Foo ");
            Assert.That.Comparable().CompareTo().IsGreaterThen(SoftString.Create("foo"), "foa", "foa ", " foa", " foa ");
        }
    }    
}
