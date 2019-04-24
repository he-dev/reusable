using System;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    internal static class ExpressionAssert
    {
        public static IConstant Equal<TValue, TExpression>(TValue expectedValue, TExpression expression, IExpressionContext context = null) where TExpression : IExpression
        {
            var expected = expectedValue is IConstant constant ? constant.Value : expectedValue; // Constant.FromValue("Expected", expectedValue);
            var debugView = TreeNode.Create(new ExpressionDebugView
            {
                Name = "Root"
            });
            context = (context ?? ExpressionContext.Empty).WithRegexComparer().WithSoftStringComparer().Set(Item.For<IDebugContext>(), x => x.DebugView, debugView);
            var actual = expression.Invoke(context);

            return
                object.Equals(expected, actual.Value is IConstant c ? c.Value : actual.Value)
                    ? actual
                    : throw DynamicException.Create("AssertFailed", CreateAssertFailedMessage(expected, actual));
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