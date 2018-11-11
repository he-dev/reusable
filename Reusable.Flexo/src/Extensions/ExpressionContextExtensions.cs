using System;
using System.Runtime.CompilerServices;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo.Extensions
{
    public static class ExpressionContextExtensions
    {
        public static TParameter Parameter<TParameter>(this IExpressionContext context, [CallerMemberName] string memberName = null) => (TParameter)context.Parameters[memberName];

        public static TExpressionContext Parameter<TParameter, TExpressionContext>(this TExpressionContext context, TParameter value, [CallerMemberName] string memberName = null)
            where TExpressionContext : IExpressionContext
        {
            context.Parameters[memberName] = value;
            return context;
        }

        public static TParameter Item<TParameter>(this IExpressionContext context, [CallerMemberName] string memberName = null)
        {
            return (TParameter)context.Parameters[memberName];
        }

        public static TExpressionContext Item<TParameter, TExpressionContext>(this TExpressionContext context, TParameter value, [CallerMemberName] string memberName = null) where TExpressionContext : IExpressionContext
        {
            context.Items[memberName] = value;
            return context;
        }

        /// <summary>
        /// Creates a new context scope.
        /// </summary>
        public static IDisposable Scope(this IExpressionContext context, IExpression expression)
        {
            return ExpressionContextScope.Push(expression, context);
        }
    }
}