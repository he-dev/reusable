using System;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    internal static class ExpressionAssert
    {
        private static readonly ITreeRenderer<string> DebugViewRenderer = new PlainTextTreeRenderer();
        
        public static IConstant Equal<TValue, TExpression>(TValue expectedValue, TExpression expression, IImmutableSession context = null) where TExpression : IExpression
        {
            var expected = expectedValue is IConstant constant ? constant.Value : expectedValue;
            
            context = (context ?? Expression.DefaultSession);           
            var actual = expression.Invoke(context);

            var debugViewString = DebugViewRenderer.Render(context.DebugView(), ExpressionDebugView.DefaultRender);

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