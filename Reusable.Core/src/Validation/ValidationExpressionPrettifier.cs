using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    // We don't want to show the exact same expression as the condition
    // because there are variables and closures that don't look pretty.
    // We replace them with more friendly names.
    internal class ValidationExpressionPrettifier : ExpressionVisitor
    {
        private readonly ParameterExpression _fromParameter;

        private readonly ParameterExpression _toParameter;

        private ValidationExpressionPrettifier(ParameterExpression fromParameter, ParameterExpression toParameter)
        {
            _fromParameter = fromParameter;
            _toParameter = toParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Equals(_fromParameter) ? _toParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Extract member name from closures.
            if (node.Expression is ConstantExpression)
            {
                return Expression.Parameter(node.Type, node.Member.Name);
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // Remove type conversion, this is change (Convert(<T>) != null) to (<T> != null)
            if (node.Operand.Type == _fromParameter.Type)
            {
                return Expression.Parameter(node.Operand.Type, _toParameter.Name);
            }

            return base.VisitUnary(node);
        }

        //public static Expression Prettify([NotNull] Expression target, [NotNull] ParameterExpression from, [NotNull] ParameterExpression to)
        //{
        //    if (target == null) throw new ArgumentNullException(nameof(target));
        //    if (from == null) throw new ArgumentNullException(nameof(from));
        //    if (to == null) throw new ArgumentNullException(nameof(to));

        //    return new ValidationExpressionPrettifier(from, to).Visit(target);
        //}

        public static Expression Prettify<T>([NotNull] Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var parameterReplacement = Expression.Parameter(typeof(T), $"<{typeof(T).Name}>");
            return new ValidationExpressionPrettifier(expression.Parameters[0], parameterReplacement).Visit(expression.Body);
        }
    }
}