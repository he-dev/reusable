using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Flexo.Expressions;

namespace Reusable.Tests.Flexo
{
    internal static class Helpers
    {
        [NotNull]
        public static IExpressionContext CreateContext() => new ExpressionContext();

        public static void ExpressionsEqual<TValue>(this Assert _, string name, TValue expectedValue, IExpression expression, IExpressionContext context = null)
        {
            context = context ?? new ExpressionContext();
            var expected = Constant.Create(name, expectedValue);
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