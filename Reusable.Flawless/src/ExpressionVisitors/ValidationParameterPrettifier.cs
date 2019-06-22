using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Flawless.ExpressionVisitors
{
    // We don't want to show the exact same expression as the condition
    // because there are variables and closures that don't look pretty.
    // We replace them with more friendly names.
    internal class ValidationParameterPrettifier : ExpressionVisitor
    {
        private readonly ParameterExpression _originalParameter;
        private readonly ParameterExpression _prettyParameter;

        private ValidationParameterPrettifier(ParameterExpression originalParameter, ParameterExpression prettyParameter)
        {
            _originalParameter = originalParameter;
            _prettyParameter = prettyParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Equals(_originalParameter) ? _prettyParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Extract member name from closures.
            return
                node.Expression is ConstantExpression
                    ? Expression.Parameter(node.Type, node.Member.Name)
                    : base.VisitMember(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // Remove type conversion, this is change (Convert(<T>) != null) to (<T> != null)
            return
                node.Operand.Type == _originalParameter.Type
                    ? Expression.Parameter(node.Operand.Type, _prettyParameter.Name)
                    : base.VisitUnary(node);
        }

        public static Expression Prettify<T>([NotNull] LambdaExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return
                expression
                    .Parameters
                    .Aggregate(expression.Body, (e, p) => new ValidationParameterPrettifier(expression.Parameters[0], CreatePrettyParameter<T>()).Visit(expression.Body));
        }

        public static ParameterExpression CreatePrettyParameter<T>()
        {
            return Expression.Parameter(typeof(T), $"<param:{typeof(T).ToPrettyString()}>");
        }
    }
}