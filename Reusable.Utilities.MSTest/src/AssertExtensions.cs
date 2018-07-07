using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.FormatProviders;
using static Reusable.StringHelper;

namespace Reusable.Utilities.MSTest
{
    public static class AssertExtensions
    {
        private static readonly IFormatProvider FormatProvider = new CompositeFormatProvider
        {
            typeof(TypeFormatProvider),
            typeof(PunctuationFormatProvider)
        };

        [NotNull]
        public static TException ThrowsExceptionWhen<TException>(this Assert assert, Action action, Func<TException, bool> filter = null) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail(Format($"Expected exception {typeof(TException):single}, but none was thrown.", FormatProvider));
            }
            catch (TException ex) when ((filter ?? (_ => true))(ex))
            {
                return ex;
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Assert.Fail(Format($"Expected exception {typeof(TException):single}, but {ex.GetType():single} was thrown.", FormatProvider));
            }

            // This is only to satisfy the compiler. We'll never reach to this as it'll always fail or return earlier.
            return null;
        }

        [NotNull]
        public static TException ThrowsExceptionWhen<TException>(this Assert assert, Func<Task> func, Func<TException, bool> filter = null) where TException : Exception
        {
            return Assert.That.ThrowsExceptionWhen(() => func().GetAwaiter().GetResult(), filter);
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