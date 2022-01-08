using System.Linq.Expressions;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless.ExpressionVisitors
{
    /// <summary>
    /// Searches for the member of the closure class.
    /// </summary>
    internal class ValidationClosureSearch : ExpressionVisitor
    {
        private MemberExpression _closure;

        public static MemberExpression FindParameter(Expression expression)
        {
            var parameterSearch = new ValidationClosureSearch();
            parameterSearch.Visit(expression);
            return parameterSearch._closure;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.Type.IsClosure())
            {
                _closure = node;
            }

            return base.VisitMember(node);
        }
    }
}