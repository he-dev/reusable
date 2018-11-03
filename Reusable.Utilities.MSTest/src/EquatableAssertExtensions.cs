using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    //public interface IEquatableAssert<T>
    //{
    //    IEquatable<T> Equatable { get; }
    //}

    //public interface IEquatableEqualsAssert<T>
    //{
    //    IEquatable<T> Equatable { get; }
    //}

    //public class EquatableAssert<T> : IEquatableAssert<T>, IEquatableEqualsAssert<T>
    //{
    //    public EquatableAssert([NotNull] IEquatable<T> equatable) => Equatable = equatable ?? throw new ArgumentNullException(nameof(equatable));
    //    public IEquatable<T> Equatable { get; }
    //}

    public interface IEquatableAssert { }
    public interface IEquatableEqualsAssert { }
    public interface IEquatableGetHashCodeAssert { }

    public static class EquatableAssertExtensions
    {
        public static IEquatableEqualsAssert EqualsMethod(this IEquatableAssert assert) => default(IEquatableEqualsAssert);

        //public static IEquatableEqualsAssert<T> Equals<T>(this IEquatableAssert<T> assert) => new EquatableAssert<T>(assert.Equatable);

        //public static void IsTrue<T>(this IEquatableAssert<T> assert, Func<IEquatable<T>, bool> predicate)
        //{
        //    Assert.IsTrue(predicate(assert.Equatable), $"The specified condition for {assert.Equatable.Stringify()} must be {bool.TrueString}.");
        //}

        public static IEquatableGetHashCodeAssert GetHashCodeMethod(this IEquatableAssert assert) => default(IEquatableGetHashCodeAssert);

        public static void IsCanonical<T>(this IEquatableAssert assert, IEquatable<T> equatable)
        {
            // ReSharper disable once EqualExpressionComparison - this is ok, we want this comparison here
            Assert.IsTrue(equatable.Equals(equatable), CreateMessage("x.Equals(x) == true"));
            Assert.IsFalse(equatable.Equals(default), CreateMessage("x.Equals(default(T)) == false"));
            Assert.IsFalse(equatable.Equals(default(object)), CreateMessage("x.Equals(default(object)) == false"));

            string CreateMessage(string requirement)
            {
                return $"{typeof(IEquatable<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
            }
        }

        //public static void IsCanonical<T>(this IEquatableAssert<T> assert)
        //{
        //    Assert.IsFalse(assert.Equatable.Equals(default(T)), CreateMessage("x.Equals(null) == False"));
        //    Assert.IsFalse(assert.Equatable.Equals(default(object)), CreateMessage("x.Equals(null) == False"));

        //    string CreateMessage(string requirement)
        //    {
        //        return $"{typeof(IEquatable<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
        //    }
        //}
    }

    public static class EquatableEqualsAssertExtensions
    {
        public static void IsTrue<T>(this IEquatableEqualsAssert assert, IEquatable<T> equatable, params T[] others)
        {
            Check(equatable, others, Assert.IsTrue);
        }

        //public static void IsTrue<T>(this IEquatableEqualsAssert<T> assert, params T[] others)
        //{
        //    Check(assert.Equatable, others, Assert.IsTrue);
        //}

        public static void IsFalse<T>(this IEquatableEqualsAssert assert, IEquatable<T> equatable, params T[] others)
        {
            Check(equatable, others, Assert.IsFalse);
        }

        //public static void IsFalse<T>(this IEquatableEqualsAssert<T> assert, params T[] others)
        //{
        //    Check(assert.Equatable, others, Assert.IsFalse);
        //}

        

        private static void Check<T>(IEquatable<T> equatable, IEnumerable<T> others, Action<bool, string> assertAction)
        {
            var i = 0;
            foreach (var other in others)
            {
                assertAction(
                    equatable.Equals(other),
                    $"{nameof(IEquatable<T>.Equals)} failed for {equatable.Stringify()} and {other.Stringify()} at [{i++}]."
                );
            }
        }
    }

    public static class EquatableGetHashCodeAssertExtensions
    {
        public static void AreEqual<T>(this IEquatableGetHashCodeAssert assert, IEquatable<T> left, IEquatable<T> right)
        {
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode(), $"{nameof(IEquatable<T>.GetHashCode)} failed for {left.Stringify()} and {right.Stringify()}.");
        }
    }
}
