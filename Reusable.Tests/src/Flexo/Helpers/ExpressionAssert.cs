using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flexo;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo.Helpers
{
    internal static class ExpressionAssert
    {
        private static readonly ITreeRenderer<string> DebugViewRenderer = new PlainTextTreeRenderer();

        public static (IConstant Result, IList<IImmutableContainer> Contexts) ExpressionEqual<TValue>
        (
            TValue expected,
            IExpression expression,
            Func<IImmutableContainer, IImmutableContainer> customizeContext = null,
            ITestOutputHelper output = default,
            bool throws = false,
            ISet<SoftString> tags = default
        )
        {
            if (throws)
            {
                var ex = Assert.Throws<ExpressionException>(() => expression.Invoke(customizeContext));
                return (default, ex.Contexts);
            }
            else
            {
                var (actual, contexts) = expression.Invoke(customizeContext);
                var debugViewString = DebugViewRenderer.Render(contexts.First().DebugView(), ExpressionDebugView.DefaultRender);

                try
                {
                    switch ((object)expected)
                    {
                        case null: break;
                        case IEnumerable collection when !(expected is string):
                            Assert.IsAssignableFrom<IEnumerable>(actual.Value);
                            Assert.Equal(collection.Cast<object>(), actual.Value<IEnumerable<IConstant>>().Values<object>());
                            if (tags.IsNotNull())
                            {
                                Assert.True(actual.Tags.SetEquals(tags));
                            }

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

                return (actual, contexts);
            }
        }
    }
}