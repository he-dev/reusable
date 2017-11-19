using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using Reusable.Tester;

namespace Reusable.Tests.Collections
{
    [TestClass]
    public class AutoEqualityTest
    {
        [TestMethod]
        public void GetHashCode_SimilarObjects_True()
        {
            var p1 = new Person { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M, Type = typeof(string) };
            var p2 = new Person { FirstName = "JOHN", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M, Type = typeof(string) };

            Assert.That.Equatable().GetHashCodeMethod().AreEqual(p1, p2);
        }

        [TestMethod]
        public void Equals_SimilarObjects_True()
        {
            var p1 = new Person { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M };
            var p2 = new Person { FirstName = "JOHN", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M };

            Assert.That.Equatable().EqualsMethod().IsTrue(p1, p2);
        }

        [TestMethod]
        public void Equals_SimilarObjects_False()
        {
            var p1 = new Person { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M };
            var p2 = new Person { FirstName = "JOHN", LastName = "DOE", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M };
            var p3 = new Person { FirstName = "JOHN", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.S };

            Assert.That.Equatable().EqualsMethod().IsFalse(p1, p2, p3);
        }

        [TestMethod]
        public void IsCanonical_True()
        {
            var p1 = new Person { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(2017, 5, 1), ShoeSize = Size.M };

            Assert.That.Equatable().IsCanonical(p1);
        }

        #region Test data

        public partial class Person
        {
            [AutoEqualityProperty(StringComparison.OrdinalIgnoreCase)]
            public string FirstName { get; set; }

            [AutoEqualityProperty]
            public string LastName { get; set; }

            [AutoEqualityProperty]
            public DateTime DateOfBirth { get; set; }

            [AutoEqualityProperty]
            public Size ShoeSize { get; set; }

            // Type does not implement the IEquatable<T> interface and thus requries the ordinary Equals.
            [AutoEqualityProperty]
            public Type Type { get; set; }
        }

        public partial class Person : IEquatable<Person>
        {
            public override int GetHashCode() => AutoEquality<Person>.Comparer.GetHashCode(this);

            public override bool Equals(object obj) => Equals(obj as Person);

            public bool Equals(Person other) => AutoEquality<Person>.Comparer.Equals(this, other);
        }

        public enum Size
        {
            S,
            M,
            L
        }

        #endregion
    }
}
