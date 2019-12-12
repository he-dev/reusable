using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;
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
            IImmutableContainer? context = default,
            ITestOutputHelper? output = default,
            bool throws = false,
            ISet<SoftString>? tags = default
        )
        {
            if (throws)
            {
                var ex = Assert.ThrowsAny<Exception>(() => expression.Invoke(context));
                return new Constant<Exception>("Exception", new[] { ex }, context);
            }
            else
            {
                var result = expression.Invoke(context);
                var debugViewString = result.Context.ToInvokeLogString(ExpressionInvokeLog.Templates.Compact);

                try
                {
                    switch ((object)expected)
                    {
                        case null: break;
                        case IEnumerable collection when !(expected is string):
                            Assert.IsAssignableFrom<IEnumerable>(result.Cast<object>().Single());
                            //Assert.Equal(collection.Cast<object>(), result.Value<IEnumerable<IConstant>>().Values<object>());
                            if (tags.IsNotNull())
                            {
                                Assert.True(result.Tags.SetEquals(tags));
                            }

                            break;
                        default:
                            Assert.Equal(expected, result.Cast<object>().Single());
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

        public static IConstant ExpressionEqual
        (
            ExpressionUseCase useCase,
            ITestOutputHelper? output = default
        )
        {
            var context = ExpressionContext.Default.BeginScope(useCase.Scope ?? ImmutableContainer.Empty);

            if (useCase.Throws)
            {
                var ex = Assert.ThrowsAny<Exception>(() => useCase.Body.Invoke(context));
                return Constant.Single(nameof(Exception), ex, context);
            }
            else
            {
                var result = useCase.Body.Invoke(context);
                var invokeLogString = result.Context.ToInvokeLogString(ExpressionInvokeLog.Templates.Compact);

                try
                {
                    Assert.Equal(useCase.Expected, result.Cast<object>());
                }
                catch (Exception)
                {
                    output?.WriteLine(invokeLogString);
                    throw;
                }

                return result;
            }
        }
    }
}