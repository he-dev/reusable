using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // There is already an ExpressionExtension so you use Helpers to easier tell them apart. 
    public static partial class ExpressionExtensions
    {
        /// <summary>
        /// Gets only enabled expressions.
        /// </summary>
        public static IEnumerable<T> Enabled<T>(this IEnumerable<T> expressions) where T : ISwitchable
        {
            return
                from expression in expressions
                where expression.Enabled
                select expression;
        }

        internal static Node<ExpressionDebugView> CreateDebugView(this IExpression expression)
        {
            return Node.Create(new ExpressionDebugView
            {
                ExpressionType = expression.GetType().ToPrettyString(),
                Name = expression.Id.ToString(),
                Description = expression.Description ?? new ExpressionDebugView().Description,
            });
        }
    }
}