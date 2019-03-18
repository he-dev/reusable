using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.Flexo;

namespace Reusable.OmniLog.Expressions
{
    public static class ExpressionContextExtensions
    {
        public static Log Log(this IExpressionContext context) => context.GetByCallerName<Log>();

        public static TExpressionContext Log<TExpressionContext>(this TExpressionContext context, Log log) where TExpressionContext : IExpressionContext
        {
            return context.SetByCallerName(log);
        }
    }

    public class GetLogLevel : Expression
    {
        public GetLogLevel() : base(nameof(GetLogLevel), ExpressionContext.Empty) { }

        protected override IExpression InvokeCore(IExpressionContext context) => Constant.FromValue(nameof(OmniLog.LogLevel), context.Log().Level());
    }

    public class GetLoggerName : Expression
    {
        public GetLoggerName() : base(nameof(GetLoggerName), ExpressionContext.Empty) { }

        protected override IExpression InvokeCore(IExpressionContext context) => Constant.FromValue(nameof(OmniLog.LogLevel), context.Log().Name());
    }

    public static class ExpressionSerializerFactory
    {
        public static IExpressionSerializer CreateSerializer(IEnumerable<Type> otherTypes = null, Action<JsonSerializer> configureSerializer = null)
        {
            var ownTypes = new[]
            {
                typeof(Reusable.OmniLog.LogLevel),
                typeof(Reusable.OmniLog.Expressions.GetLoggerName),
                typeof(Reusable.OmniLog.Expressions.GetLogLevel),
            };

            return new ExpressionSerializer
            (
                customTypes: ownTypes.Concat(otherTypes ?? Enumerable.Empty<Type>()),
                configureSerializer: serializer =>
                {
                    serializer.Converters.Add(new LogLevelConverter());
                    configureSerializer?.Invoke(serializer);
                }
            );
        }
    }
}