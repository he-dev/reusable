using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public static partial class ExpressionContext
    {
        public static IImmutableContainer BeginScope(this IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return (scope ?? ImmutableContainer.Empty).SetItem(Parent, context);
        }

        public static IImmutableContainer BeginScopeWithArg(this IImmutableContainer context, IConstant arg)
        {
            return context.BeginScope().SetItem(Arg, arg);
        }
        
        public static IImmutableContainer BeginScopeWithArg(this IImmutableContainer context, string id, object value)
        {
            return context.BeginScopeWithArg(Constant.Single(id, value));
        }

        /// <summary>
        /// Enumerates expression scopes from last to first. 
        /// </summary>
        public static IEnumerable<IImmutableContainer> Scopes(this IImmutableContainer context)
        {
            while (context is {})
            {
                yield return context;
                context = context.GetItemOrDefault(Parent);
            }
        }
    }
}