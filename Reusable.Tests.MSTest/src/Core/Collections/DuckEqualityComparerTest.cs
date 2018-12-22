using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;

namespace Reusable.Tests.Collections
{
    using static Assert;

    [TestClass]
    public class DuckEqualityComparerTest
    {
        [TestMethod]
        public void Equals_CanCompareTwoNamedTypes()
        {
            var p0 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p1 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p2 = new PersonLib2 { FirstName = "JOHN", LastName = "Doe" };
            var p3 = new PersonLib2 { FirstName = "Joh", LastName = "Doe" };
            var p4 = new PersonLib2 { FirstName = default, LastName = "Doe" };

            var comparer =
                new DuckEqualityComparerBuilder<PersonLib1, PersonLib2>()
                    .Compare(x => x.FirstName, y => y.FirstName, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName, y => y.LastName, StringComparer.OrdinalIgnoreCase)
                    .Build();

            //IsTrue(comparer.Equals(p0, p1));
            IsTrue(comparer.Equals(p1, p1));
            IsTrue(comparer.Equals(p2, p2));

            IsTrue(comparer.Equals(p1, p2));
            //IsTrue(comparer.Equals(p2, p1));

            IsFalse(comparer.Equals(p1, p3));
            IsFalse(comparer.Equals(p3, p1));
            IsFalse(comparer.Equals(p1, p4));
        }

        [TestMethod]
        public void Equals_DisallowsToCompareSameTypes()
        {
            var p0 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p1 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p2 = new PersonLib2 { FirstName = "JOHN", LastName = "Doe" };
            var p3 = new PersonLib2 { FirstName = "Joh", LastName = "Doe" };
            var p4 = new PersonLib2 { FirstName = default, LastName = "Doe" };

            var comparer =
                new DuckEqualityComparerBuilder<PersonLib1, PersonLib1>()
                    .Compare(x => x.FirstName, y => y.FirstName, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName, y => y.LastName, StringComparer.OrdinalIgnoreCase)
                    .Build();

            //IsTrue(comparer.Equals(p0, p1));
            IsTrue(comparer.Equals(p1, p1));
            IsTrue(comparer.Equals(p2, p2));
        }

        [TestMethod]
        public void Equals_CanCompareTwoAnonymousTypes()
        {
            var comparer =
                DuckEqualityComparerBuilder
                    .Create(
                        new { FirstName1 = default(string), LastName1 = default(string) },
                        new { FirstName2 = default(string), LastName2 = default(string) }
                    )
                    .Compare(x => x.FirstName1, y => y.FirstName2, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName1, y => y.LastName2, StringComparer.OrdinalIgnoreCase)
                    .Build();

            IsTrue(
                comparer.Equals(
                    new { FirstName1 = "John", LastName1 = "Doe" },
                    new { FirstName2 = "JOHN", LastName2 = "DOE" }
                )
            );

            IsFalse(
                comparer.Equals(
                    new { FirstName1 = "Johny", LastName1 = "Dope" },
                    new { FirstName2 = "JOHN", LastName2 = "DOE" }
                )
            );
        }

        [TestMethod]
        public void Equals_CanCompareNamedAndAnonymousTypes()
        {
            var comparer =
                DuckEqualityComparerBuilder
                    .Create(
                        default(PersonLib1),
                        new { FirstName2 = default(string), LastName2 = default(string) }
                    )
                    .Compare(x => x.FirstName, y => y.FirstName2, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName, y => y.LastName2, StringComparer.OrdinalIgnoreCase)
                    .Build();

            IsTrue(
                comparer.Equals(
                    new PersonLib1 { FirstName = "John", LastName = "Doe" },
                    new { FirstName2 = "JOHN", LastName2 = "DOE" }
                )
            );

            IsFalse(
                comparer.Equals(
                    new PersonLib1 { FirstName = "Johny", LastName = "Dope" },
                    new { FirstName2 = "JOHN", LastName2 = "DOE" }
                )
            );
        }

        [TestMethod]
        public void Equals_CanCompareWithCustomEqualsAndIsNotCommutative()
        {
            var comparer =
                new DuckEqualityComparerBuilder<PersonLib1, PersonLib2>(isCanonical: false)
                    .Compare(x => x.FirstName, y => y.FirstName, (x, y) => x.StartsWith(y, StringComparison.OrdinalIgnoreCase))
                    .Compare(x => x.LastName, y => y.LastName, (x, y) => x.EndsWith(y, StringComparison.OrdinalIgnoreCase))
                    .Build();

            var p1 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p2 = new PersonLib2 { FirstName = "JOHN", LastName = "Doe" };
            var p3 = new PersonLib2 { FirstName = "Joh", LastName = "Doe" };
            var p4 = new PersonLib2 { FirstName = default, LastName = "Doe" };

            IsTrue(comparer.Equals(p1, p1));
            IsTrue(comparer.Equals(p2, p2));

            IsTrue(comparer.Equals(p1, p2));
            IsTrue(comparer.Equals(p1, p3));
            IsFalse(comparer.Equals(p1, p4));

            ThrowsException<ArgumentException>(() => comparer.Equals(p2, p1));
        }
    }

    class PersonLib1
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }

    class PersonLib2
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}