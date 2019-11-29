using System;
using System.Collections.Generic;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public static partial class ExpressionContext
    {
        public static IImmutableContainer UpdateItem<T>(this IImmutableContainer context, Selector<T> selector, Action<T> update)
        {
            var item = context.FindItem(selector);
            update(item);
            return context;
        }
        
        public static string ToInvokeLogString(this IImmutableContainer context, RenderTreeNodeValueDelegate<IExpression, NodePlainView> template)
        {
            return context.FindItem(InvokeLog).Views(template).Render();
        }
    }

    public static class EqualityComparerExtensions
    {
        public static IEqualityComparer<object> Objectify<T>(this IEqualityComparer<T> comparer)
        {
            return EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => comparer.Equals((T)left, (T)right),
                getHashCode: (obj) => comparer.GetHashCode((T)obj)
            );
        }
    }
}