using System;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Flexo;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    internal static class ExpressionAssert
    {
        public static void Equal<TValue, TExpression>(TValue expectedValue, TExpression expression, IExpressionContext context = null) where TExpression : IExpression
        {
            var expected = expectedValue is IConstant constant ? constant : Constant.FromValue("Expected", expectedValue);
            var actual = expression.Invoke(context ?? ExpressionContext.Empty);

            if (!expected.Equals(actual))
            {
                throw DynamicException.Create("AssertFailed", CreateAssertFailedMessage(expected, actual));
            }
        }

        private static string CreateAssertFailedMessage(object expected, object actual)
        {
            return
                $"{Environment.NewLine}" +
                $"» Expected:{Environment.NewLine}{expected}{Environment.NewLine}" +
                $"» Actual:{Environment.NewLine}{actual}" +
                $"{Environment.NewLine}";
        }
    }
}