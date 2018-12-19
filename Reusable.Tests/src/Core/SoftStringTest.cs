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
        public void ctor_DisallowsNullValue()
        {
            // ReSharper disable once AssignNullToNotNullAttribute - we want it to be null
            Assert.That.Throws<ArgumentNullException>(() => SoftString.Create(null));
        }
        
        [TestMethod]
        public void ToString_ReturnsTrimmedValue()
        {
            Assert.AreEqual(string.Empty, new SoftString().ToString());
            Assert.AreEqual("foo", new SoftString(" foo ").ToString());            
        }        

        [TestMethod]
        public void this_CanGetChar()
        {
            Assert.AreEqual('r', SoftString.Create("bar")[2]);
        }

        [TestMethod]
        public void Length_GetsLengthOfTrimmedValue()
        {
            Assert.AreEqual(3, SoftString.Create(" fOo ").Length);
        }

        [TestMethod]
        public void Equals_CanIdentifySimilarValues()
        {
            Assert.That.Equatable().EqualsMethod().IsTrue(SoftString.Create("foo"), "foo", "fOo", "foo ", " fOo", " foo ");
        }

        [TestMethod]
        public void Equals_CanIdentifyDifferentValues()
        {
            Assert.That.Equatable().EqualsMethod().IsFalse(SoftString.Create("foo"), "bar", "bar ", " bar", " bar ");
        }

        [TestMethod]
        public void opEqual_CanIdentifySimilarValues()
        {
            Assert.That.BinaryOperator().Equality().IsTrue(default(SoftString), default(string));
            Assert.That.BinaryOperator().Equality().IsTrue(SoftString.Create("foo"), "foo", "fOo", "foo ", " fOo", " foo ");
        }

        [TestMethod]
        public void opEqual_CanIdentifyDifferentValues()
        {
            Assert.That.BinaryOperator().Equality().IsFalse(SoftString.Create("foo"), "bar", " fOob");
        }

        [TestMethod]
        public void opExplicit_CanExplicitlyCastToString()
        {
            Assert.That.UnaryOperator().Convert(SoftString.Create("foo")).IsEqual("foo");
        }

        [TestMethod]
        public void opImplicit_CanImplicitlyCastFromString()
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
            Assert.That.Comparable().CompareTo().IsLessThan(SoftString.Create("fao"), "foz", "foz ", " foz", " foz ");
            Assert.That.Comparable().CompareTo().IsEqualTo(SoftString.Create("foo"), "foo", "fOo", "fOo ", " fOo", " Foo ");
            Assert.That.Comparable().CompareTo().IsGreaterThan(SoftString.Create("foo"), "foa", "foa ", " foa", " foa ");
        }
    }    
}
