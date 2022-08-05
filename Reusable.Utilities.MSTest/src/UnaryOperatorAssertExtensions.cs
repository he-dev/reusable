using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Marbles.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface IUnaryOperatorAssert { }

    public interface IUnaryOperatorConvertAssert<out T>
    {
        T Value { get; }
    }

    public static class UnaryOperatorAssertExtensions
    {
        public static IUnaryOperatorConvertAssert<TSource> Convert<TSource>(this IUnaryOperatorAssert assert, TSource source)
        {
            return new UnaryOperatorConvertAssert<TSource>(source);
        }
    }

    public class UnaryOperatorConvertAssert<T> : IUnaryOperatorConvertAssert<T>
    {
        public UnaryOperatorConvertAssert(T value) => Value = value;
        public T Value { get; }
    }

    public static class UnaryOperatorConvertAssertExtensions
    {
        public static void IsEqual<TSource, TExpected>(this IUnaryOperatorConvertAssert<TSource> assert, TExpected expected)
        {
            var convertExpr =
                Expression.Convert(
                    Expression.Constant(assert.Value),
                    typeof(TExpected)
                );

            var convertFunc = Expression.Lambda<Func<TExpected>>(convertExpr).Compile();

            try
            {
                var actual = convertFunc();

                Assert.AreEqual(
                    expected,
                    actual,
                    $"Conversion from {typeof(TSource).Name.QuoteWith("'")} to {typeof(TExpected).Name.QuoteWith("'")} failed. " +
                    $"Expected: {expected.Stringify()}, Actual: {assert.Stringify()}.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Conversion from {typeof(TSource).Name.QuoteWith("'")} to {typeof(TExpected).Name.QuoteWith("'")} threw an exception: {ex}");
            }
        }
    }
}
