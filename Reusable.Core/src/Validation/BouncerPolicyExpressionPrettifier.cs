using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Validation
{
    // We don't want to show the exact same expression as the condition
    // because there are variables and closures that don't look pretty.
    // We replace them with more friendly names.
    internal class BouncerPolicyExpressionPrettifier : ExpressionVisitor
    {
        private readonly ParameterExpression _originalParameter;

        private readonly ParameterExpression _replacementParameter;

        private BouncerPolicyExpressionPrettifier(ParameterExpression originalParameter, ParameterExpression replacementParameter)
        {
            _originalParameter = originalParameter;
            _replacementParameter = replacementParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Equals(_originalParameter) ? _replacementParameter : base.VisitParameter(node);
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
            if (node.Operand.Type == _originalParameter.Type)
            {
                return Expression.Parameter(node.Operand.Type, _replacementParameter.Name);
            }

            return base.VisitUnary(node);
        }

        public static Expression Prettify<T>([NotNull] Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var replacementParameter = Expression.Parameter(typeof(T), $"<{typeof(T).ToPrettyString()}>");
            return new BouncerPolicyExpressionPrettifier(expression.Parameters[0], replacementParameter).Visit(expression.Body);
        }
    }
}