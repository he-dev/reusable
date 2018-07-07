using System;
using System.Text.RegularExpressions;
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

        #region Specialized asserts

        public static IEquatableAssert Equatable(this Assert assert) => default(IEquatableAssert);

        public static IComparableAssert Comparable(this Assert assert) => default(IComparableAssert);

        public static IBinaryOperatorAssert BinaryOperator(this Assert assert) => default(IBinaryOperatorAssert);

        public static IUnaryOperatorAssert UnaryOperator(this Assert assert) => default(IUnaryOperatorAssert);

        public static ILookupAssert Lookup(this Assert assert) => default(ILookupAssert);

        public static ITypeAssert Type<T>(this Assert assert) => new TypeAssert(typeof(T));

        public static ICollectionAssert Collection(this Assert assert) => default;

        #endregion

        #region Throw overloads

        public static TException Throws<TException>(this Assert assert, Action action, Action<ExceptionFilterBuilder<TException>> configureFilter) where TException : Exception
        {
            var filter = new ExceptionFilterBuilder<TException>();
            configureFilter(filter);

            try
            {
                action();
                Fail(System.Type.Missing);
            }
            catch (TException ex) when (filter.IsMatch(ex))
            {
                return ex;
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Fail(ex.GetType());
            }

            // This is only to satisfy the compiler. We'll never reach to this as it'll always fail or return earlier.
            return default;

            void Fail(object thrownExceptionType)
            {
                Assert.Fail(
                    Format($"{Environment.NewLine}» Expected: <{typeof(TException)}>", FormatProvider) +
                    Format($"{Environment.NewLine}» Actual: <{(thrownExceptionType == System.Type.Missing ? "none" : thrownExceptionType)}>", FormatProvider));
            }
        }

        public static TException Throws<TException>(this Assert assert, Action action) where TException : Exception
        {
            return assert.Throws<TException>(action, _ => { });
        }

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

    //public interface IThrows<TException> where TException : Exception { }

    public class ExceptionFilterBuilder<TException> where TException : Exception
    {
        [CanBeNull]
        private string _namePattern;

        [CanBeNull]
        private string _messagePattern;

        [NotNull]
        public ExceptionFilterBuilder<TException> WhenName([NotNull, RegexPattern] string namePattern)
        {
            _namePattern = namePattern ?? throw new ArgumentNullException(nameof(namePattern));
            return this;
        }

        [NotNull]
        public ExceptionFilterBuilder<TException> WhenMessage([NotNull, RegexPattern] string messagePattern)
        {
            _messagePattern = messagePattern ?? throw new ArgumentNullException(nameof(messagePattern));
            return this;
        }

        internal bool IsMatch(TException exception) => IsNameMatch(exception) && IsMessageMatch(exception);

        private bool IsNameMatch(TException exception) => _namePattern is null || Regex.IsMatch(exception.GetType().Name, _namePattern);

        private bool IsMessageMatch(TException exception) => _messagePattern is null || Regex.IsMatch(exception.Message, _messagePattern);
    }
}