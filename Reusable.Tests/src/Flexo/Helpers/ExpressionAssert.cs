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
        public static ExpressionInvokeResult ExpressionEqual<TValue>
        (
            TValue expected,
            IExpression expression,
            Func<IImmutableContainer, IImmutableContainer> alterContext = null,
            ITestOutputHelper output = default,
            bool throws = false,
            ISet<SoftString> tags = default
        )
        {
            if (throws)
            {
                var ex = Assert.Throws<ExpressionException>(() => expression.Invoke(alterContext));
                return new ExpressionInvokeResult { Contexts = ex.Contexts };
            }
            else
            {
                var result = expression.Invoke(alterContext);
                var debugViewString = result.DebugViewToString(ExpressionDebugView.Templates.Compact);

                try
                {
                    switch ((object)expected)
                    {
                        case null: break;
                        case IEnumerable collection when !(expected is string):
                            Assert.IsAssignableFrom<IEnumerable>(result.Constant.Value);
                            Assert.Equal(collection.Cast<object>(), result.Constant.Value<IEnumerable<IConstant>>().Values<object>());
                            if (tags.IsNotNull())
                            {
                                Assert.True(result.Constant.Tags.SetEquals(tags));
                            }

                            break;
                        default:
                            Assert.Equal(expected, result.Constant.Value);
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