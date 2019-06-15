using System.Collections;
using Reusable.Cryptography;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Tests.Cryptography
{
    public class FingerprintBuilderTest
    {
        /*
         = Two objects with equal values
         ! Calculate fingerprints
         > Objects' fingerprints are equal
         */
        [Fact]
        public void Fingerprints_are_equal_for_objects_with_equal_values()
        {
            var calculateFingerprint =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Name)
                    .Add(x => x.Age)
                    .Build(SHA256.ComputeHash);

            var fingerprint1 = calculateFingerprint(new Person { Name = "John", Age = 40 });
            var fingerprint2 = calculateFingerprint(new Person { Name = "John", Age = 40 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint2));
        }

        /*
         = Two objects with similar values
         ! Calculate fingerprints
         > Objects' fingerprints are equal
         */
        [Fact]
        public void Fingerprints_are_equal_for_objects_with_similar_values()
        {
            var calculateFingerprint =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Name, StringOptions.IgnoreCase | StringOptions.IgnoreWhitespace)
                    .Add(x => x.Age)
                    .Build(SHA256.ComputeHash);

            var fingerprint1 = calculateFingerprint(new Person { Name = "John", Age = 40 });
            var fingerprint2 = calculateFingerprint(new Person { Name = " JOHN   ", Age = 40 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint2));
        }

        /*
         = Two objects with different values
         ! Calculate fingerprints
         > Objects' fingerprints are not equal
         */
        [Fact]
        public void Fingerprints_are_not_equal_for_objects_with_different_values()
        {
            var calculateFingerprint =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Name)
                    .Add(x => x.Age)
                    .Build(SHA256.ComputeHash);

            var fingerprint1 = calculateFingerprint(new Person { Name = "John", Age = 40 });
            var fingerprint2 = calculateFingerprint(new Person { Name = "Johnny", Age = 41 });

            Assert.False(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint2));
        }
        
        /*
         = Two objects with same values
         & one fingerprint calculator for XY
         & one fingerprint calculator for YX
         ! Calculate fingerprints
         > Objects' fingerprints are equal
         */
        [Fact]
        public void Fingerprints_are_equal_irrespective_of_property_order()
        {
            var calculateFingerprint1 =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Name)
                    .Add(x => x.Age)
                    .Build(SHA256.ComputeHash);
            
            var calculateFingerprint2 =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Age)
                    .Add(x => x.Name)
                    .Build(SHA256.ComputeHash);

            var fingerprint1 = calculateFingerprint1(new Person { Name = "John", Age = 40 });
            var fingerprint2 = calculateFingerprint2(new Person { Name = "John", Age = 40 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint2));
        }        

        /*
         = Two objects with null values
         ! Calculate fingerprints
         > Objects' fingerprints are equal
         & null values are ignored
         */
        [Fact]
        public void Ignores_null_values()
        {
            var calculateFingerprint =
                FingerprintBuilder
                    .For<Person>()
                    .Add(x => x.Name)
                    .Add(x => x.Age)
                    .Build(SHA256.ComputeHash);

            var fingerprint1 = calculateFingerprint(new Person { Name = null, Age = 40 });
            var fingerprint2 = calculateFingerprint(new Person { Name = null, Age = 40 });
            var fingerprint3 = calculateFingerprint(new Person { Name = "John", Age = 40 });

            Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint2));
            Assert.False(StructuralComparisons.StructuralEqualityComparer.Equals(fingerprint1, fingerprint3));
        }               

        [Fact]
        public void Throws_when_builder_does_not_have_any_value_selectors()
        {
            Assert.ThrowsAny<DynamicException>(() => FingerprintBuilder.For<Person>().Build(SHA256.ComputeHash));
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}