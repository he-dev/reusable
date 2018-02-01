using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public static class AssertExtensions
    {
        public static T ThrowsExceptionFiltered<T>(this Assert assert, Action action, Func<T, bool> filter = null) where T : Exception
        {
            filter = filter ?? (ex => true);

            try
            {
                action();
                Assert.Fail($"Expected exception {typeof(T).Name.QuoteWith("'")}, but none was thrown.");
            }
            catch (T ex) when (filter(ex))
            {
                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected exception '{typeof(T).Name}', but {ex.GetType().Namespace.QuoteWith("'")} was thrown.");
            }

            // This is only to satisfy the compiler. We'll never reach to this as it'll always fail or return earlier.
            return null;
        }

        #region Specialized asserts

        public static IEquatableAssert Equatable(this Assert assert) => default(IEquatableAssert);

        public static IComparableAssert Comparable(this Assert assert) => default(IComparableAssert);

        public static IBinaryOperatorAssert BinaryOperator(this Assert assert) => default(IBinaryOperatorAssert);

        public static IUnaryOperatorAssert UnaryOperator(this Assert assert) => default(IUnaryOperatorAssert);

        public static ILookupAssert Lookup(this Assert assert) => default(ILookupAssert);

        public static ITypeAssert Type<T>(this Assert assert) => new TypeAssert(typeof(T));

        public static ICollectionAssert Collection(this Assert asser) => default(ICollectionAssert);

        #endregion

        //public static void IsLessThen(this Assert assert, int expectedLessThen, int actual, Func<int, string> createMessage = null)
        //{
        //    Assert.IsTrue(actual < expectedLessThen, createMessage?.Invoke(actual));
        //}

        //public static void IsGreaterThen(this Assert assert, int expectedGreaterThen, int actual, Func<int, string> createMessage = null)
        //{
        //    Assert.IsTrue(actual > expectedGreaterThen, createMessage?.Invoke(actual));
        //}

        //public static void IsEqual(this Assert assert, int expectedEqual, int actual, Func<int, string> createMessage = null)
        //{
        //    Assert.IsTrue(actual == expectedEqual, createMessage?.Invoke(actual));
        //}

        //public static IEquatableAssert<T> Equatable<T>(this Assert assert, IEquatable<T> equatable) => new EquatableAssert<T>(equatable);
    }
}