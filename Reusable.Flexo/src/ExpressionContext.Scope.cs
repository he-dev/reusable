using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public static partial class ExpressionContext
    {
        public static IImmutableContainer BeginScope(this IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return (scope ?? ImmutableContainer.Empty).SetItem(ExpressionContext.Parent, context);
        }

        public static IImmutableContainer BeginScopeWithArg(this IImmutableContainer context, object arg)
        {
            return context.BeginScope(ImmutableContainer.Empty.SetItem(ExpressionContext.Arg, arg));
        }

//        public static IImmutableContainer BeginScopeWithItem(this IImmutableContainer context, object item)
//        {
//            return context.BeginScope(ImmutableContainer.Empty.SetItem(ExpressionContext.Item, item));
//        }

        /// <summary>
        /// Enumerates expression scopes from last to first. 
        /// </summary>
        public static IEnumerable<IImmutableContainer> Scopes(this IImmutableContainer context)
        {
            do
            {
                yield return context;
                context = context.GetItemOrDefault(ExpressionContext.Parent);
            } while (context.IsNotNull());
        }
    }
}