using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.FormatProviders;
using Xunit;

namespace Reusable.Utilities.Xunit.Extensions
{
    public static class AssertExtensions
    {
        private static readonly IFormatProvider FormatProvider = new CompositeFormatProvider
        {
            typeof(TypeFormatProvider),
            typeof(PunctuationFormatProvider)
        };
        
        public static void IsNullOrEmpty(this Assert assert, string value) => Assert.True(value.IsNullOrEmpty());

        #region Specialized asserts

        public static IEquatableAssert Equatable(this Assert assert) => default;

        public static IComparerAssert Comparer(this Assert assert) => default;

        public static IComparableAssert Comparable(this Assert assert) => default;

        public static IBinaryOperatorAssert BinaryOperator(this Assert assert) => default;

        public static IUnaryOperatorAssert UnaryOperator(this Assert assert) => default;

        //public static ILookupAssert Lookup(this Assert assert) => default;

        //public static ITypeAssert Type<T>(this Assert assert) => new TypeAssert(typeof(T));

        //public static ICollectionAssert Collection(this Assert assert) => default;

        #endregion

        #region Throw overloads

//        public static TException Throws<TException>(
//            this Assert assert,
//            Action action,
//            [CanBeNull] Action<ExceptionFilterBuilder> filter = default,
//            [CanBeNull] Action<ExceptionFilterBuilder> inner = default
//        ) where TException : Exception
//        {
//            var filterBuilder = new ExceptionFilterBuilder();
//            var innerBuilder = new ExceptionFilterBuilder();
//
//            filter?.Invoke(filterBuilder);
//            inner?.Invoke(innerBuilder);
//
//            try
//            {
//                action();
//                Fail(default);
//            }
//            catch (TException ex) when (filterBuilder.When(ex))
//            {
//                if (!ex.SelectMany().Any(t => innerBuilder.When(t.Exception)))
//                {
//                    Fail(ex, true);
//                }
//
//                return ex;
//            }
//            catch (AssertFailedException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                Fail(ex);
//            }
//
//            // This is only to satisfy the compiler. We'll never reach to this as it'll always fail or return earlier.
//            return default;
//
//            void Fail(Exception thrownException, bool innerFailed = false)
//            {
//                var messages = new[]
//                {
//                    string.Empty,
//                    !innerFailed
//                        ? Format($"» Expected: <{typeof(TException)}> when(name: '{filterBuilder.NamePattern ?? "ANY"}', message: '{filterBuilder.MessagePattern ?? "ANY"}')", FormatProvider)
//                        : Format($"» Expected: <<Inner{nameof(Exception)}>> when(name: '{innerBuilder.NamePattern ?? "ANY"}', message: '{innerBuilder.MessagePattern ?? "ANY"}')", FormatProvider),
//                    Format($"» Actual: <{thrownException?.GetType() ?? (object) "none"}> {(thrownException?.Message)}", FormatProvider),
//                };
//
//                Assert.Fail(messages.Join(Environment.NewLine));
//            }
//        }

//        public static TException Throws<TException>(this Assert assert, Action action) where TException : Exception
//        {
//            return assert.Throws<TException>(action, _ => { });
//        }

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

    public class ExceptionFilterBuilder
    {
        [CanBeNull]
        public string NamePattern { get; private set; }

        [CanBeNull]
        public string MessagePattern { get; private set; }

        public void When([CanBeNull, RegexPattern] string name = default, [CanBeNull, RegexPattern] string message = default)
        {
            NamePattern = name;
            MessagePattern = message;
        }

        internal bool When<T>(T exception) where T : Exception => IsNameMatch(exception) && IsMessageMatch(exception);

        private bool IsNameMatch<T>(T exception) where T : Exception => NamePattern is null || Regex.IsMatch(exception.GetType().Name, NamePattern);

        private bool IsMessageMatch<T>(T exception) where T : Exception => MessagePattern is null || Regex.IsMatch(exception.Message, MessagePattern);

        public override string ToString() => $"name: {NamePattern} && message: {MessagePattern}";
    }
}