using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.Flexo.Helpers
{
    internal static class ExpressionAssert
    {
        public static IConstant ExpressionEqual<TValue>
        (
            TValue expected,
            IExpression expression,
            IImmutableContainer? scope = default,
            ITestOutputHelper? output = default,
            bool throws = false,
            ISet<SoftString>? tags = default
        )
        {
            var context = ExpressionContext.Default.BeginScope(scope ?? ImmutableContainer.Empty);

            if (throws)
            {
                var ex = Assert.Throws<ExpressionException>(() => expression.Invoke(context));
                return new Constant<Exception>("Exception", ex, context);
            }
            else
            {
                var result = expression.Invoke(context);
                var debugViewString = result.DebugViewToString(ExpressionDebugView.Templates.Compact);

                try
                {
                    switch ((object)expected)
                    {
                        case null: break;
                        case IEnumerable collection when !(expected is string):
                            Assert.IsAssignableFrom<IEnumerable>(result.Value);
                            Assert.Equal(collection.Cast<object>(), result.Value<IEnumerable<IConstant>>().Values<object>());
                            if (tags.IsNotNull())
                            {
                                Assert.True(result.Tags.SetEquals(tags));
                            }

                            break;
                        default:
                            Assert.Equal(expected, result.Value);
                            break;
                    }
                }
                catch (Exception)
                {
                    output?.WriteLine(debugViewString);
                    throw;
                }

                return result;
            }
        }
    }
}