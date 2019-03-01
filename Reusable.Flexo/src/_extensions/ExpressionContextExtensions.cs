using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo
{
    public static class ExpressionContextExtensions
    {
        [CanBeNull]
        public static TParameter Item<TParameter>(this IExpressionContext context, [CallerMemberName] string itemName = null)
        {
            return (TParameter)context[itemName];
        }
        
        [NotNull]
        public static TExpressionContext Item<TParameter, TExpressionContext>(this TExpressionContext context, TParameter value, [CallerMemberName] string itemName = null) 
            where TExpressionContext : IExpressionContext
        {
            context.SetItem(itemName, value);
            return context;
        }

        [NotNull]
        public static IDisposable Scope(this IExpressionContext context, IExpression expression) => ExpressionContextScope.Push(expression, context);
    }
}