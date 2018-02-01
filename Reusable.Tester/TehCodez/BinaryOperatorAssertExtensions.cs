using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface IBinaryOperatorAssert { }

    [BinaryOperator("==")]
    public interface IBinaryOperatorEqualityAssert { }

    [BinaryOperator("!=")]
    public interface IBinaryOperatorInequalityAssert { }

    public class BinaryOperatorAttribute : Attribute
    {
        private readonly string _op;
        public BinaryOperatorAttribute(string op) => _op = op;
        public override string ToString() => _op;
    }

    public static class BinaryOperatorAssertExtensions
    {
        public static IBinaryOperatorEqualityAssert Equality(this IBinaryOperatorAssert assert) => default(IBinaryOperatorEqualityAssert);
        public static IBinaryOperatorInequalityAssert Inequality(this IBinaryOperatorAssert assert) => default(IBinaryOperatorInequalityAssert);
    }

    public static class BinaryOperatorEqualityAssertExtensions
    {
        public static void IsTrue<T>(this IBinaryOperatorEqualityAssert assert, T left, params T[] others)
        {
            BinaryOperator<IBinaryOperatorEqualityAssert>.Check(left, others, BinaryOperation<T>.Equal, Assert.IsTrue);
        }

        public static void IsFalse<T>(this IBinaryOperatorEqualityAssert assert, T left, params T[] others)
        {
            BinaryOperator<IBinaryOperatorEqualityAssert>.Check(left, others, BinaryOperation<T>.Equal, Assert.IsFalse);
        }
    }

    public static class BinaryOperatorInequalityAssertExtensions
    {
        public static void IsTrue<T>(this IBinaryOperatorInequalityAssert assert, T left, IEnumerable<T> others)
        {
            BinaryOperator<IBinaryOperatorInequalityAssert>.Check(left, others, BinaryOperation<T>.NotEqual, Assert.IsTrue);
        }

        public static void IsFalse<T>(this IBinaryOperatorInequalityAssert assert, T left, IEnumerable<T> others)
        {
            BinaryOperator<IBinaryOperatorInequalityAssert>.Check(left, others, BinaryOperation<T>.NotEqual, Assert.IsFalse);
        }
    }

    internal static class BinaryOperator<TBinaryOperator>
    {
        public static void Check<T>(T left, IEnumerable<T> others, Func<T, T, bool> predicate, Action<bool, string> assert)
        {
            var opName = typeof(TBinaryOperator).GetCustomAttribute<BinaryOperatorAttribute>();
            var i = 0;
            foreach (var right in others)
            {
                assert(
                    predicate(left, right),
                    $"Operator {typeof(T).Name.QuoteWith("'")} {opName} {typeof(T).Name.QuoteWith("'")} " +
                    $"failed for {left.Stringify()} {opName} {right.Stringify()} at [{i++}]."
                );
            }
        }
    } 
}
