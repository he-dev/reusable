using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // There is already an ExpressionExtension so you use Helpers to easier tell them apart. 
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets only enabled expressions.
        /// </summary>
        public static IEnumerable<T> Enabled<T>(this IEnumerable<T> expressions) where T : ISwitchable
        {
            return
                from e in expressions
                where e.Enabled
                select e;
        }

        /// <summary>
        /// Invokes enabled expressions.
        /// </summary>
        public static IEnumerable<IConstant> Invoke(this IEnumerable<IExpression> expressions, IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return
                from e in expressions.Enabled()
                select e.Invoke(context, scope);
        }

        public static IConstant Invoke(this IExpression expression, IImmutableContainer context, IImmutableContainer? scope = default)
        {
            return expression.Invoke(context.BeginScope(scope));
        }
    }
}