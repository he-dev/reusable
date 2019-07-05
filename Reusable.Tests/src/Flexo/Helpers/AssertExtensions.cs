using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo.Helpers
{
    internal static class AssertExtensions
    {
        private static readonly ITreeRenderer<string> DebugViewRenderer = new PlainTextTreeRenderer();

        public static (IConstant Result, IImmutableContainer Context) ExpressionEqual<TValue>
        (
            this Assert assert,
            TValue expected,
            IExpression expression,
            Func<IImmutableContainer, IImmutableContainer> customizeContext = null,
            ITestOutputHelper output = default,
            bool throws = false
        )
        {
            var expressionInvoker = new ExpressionInvoker();

            if (throws)
            {
                var (actual, context) = expressionInvoker.Invoke(expression, customizeContext);
                Assert.IsAssignableFrom<Exception>(actual.Value);
                return (actual, context);
            }
            else
            {
                var (actual, context) = expressionInvoker.Invoke(expression, customizeContext);
                var debugViewString = DebugViewRenderer.Render(context.DebugView(), ExpressionDebugView.DefaultRender);

                try
                {
                    switch ((object)expected)
                    {
                        case null: break;
                        case IEnumerable collection when !(expected is string):
                            Assert.IsAssignableFrom<IEnumerable>(actual.Value);
                            Assert.Equal(collection.Cast<object>(), actual.Value<IEnumerable<IConstant>>().Values<object>());
                            break;
                        default:
                            Assert.Equal(expected, actual.Value);
                            break;
                    }
                }
                catch (Exception)
                {
                    output?.WriteLine(debugViewString);
                    throw;
                }

                return (actual, context);
            }
        }
    }
}