using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Flexo;

namespace Reusable.Tests.Flexo
{
    internal static class Helpers
    {
        public static void ExpressionsEqual<TValue, TExpression>(this Assert _, TValue expectedValue, TExpression expression, IExpressionContext context = null) where TExpression : IExpression
        {
            context = context ?? new ExpressionContext();
            var expected = Constant.FromValue(expression.Name, expectedValue);
            var actual = expression.Invoke(context);

            if (!expected.Equals(actual))
            {
                throw new AssertFailedException(CreateAssertFailedMessage(expected, actual));
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