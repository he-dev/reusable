using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
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

        public static IImmutableContainer BeginScopeWithArg(this IImmutableContainer context, object arg)
        {
            return context.BeginScope().SetItem(Arg, arg);
        }

        /// <summary>
        /// Enumerates expression scopes from last to first. 
        /// </summary>
        public static IEnumerable<IImmutableContainer> Scopes(this IImmutableContainer context)
        {
            do
            {
                yield return context;
                context = context.GetItemOrDefault(Parent);
            } while (context.IsNotNull());
        }
    }
}