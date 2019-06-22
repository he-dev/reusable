using System.Linq.Expressions;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless.ExpressionVisitors
{
    /// <summary>
    /// Injects the specified parameter to replace the closure.
    /// </summary>
    public class ValidationParameterInjector : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        private ValidationParameterInjector(ParameterExpression parameter) => _parameter = parameter;

        public static Expression InjectParameter(Expression expression, ParameterExpression parameter)
        {
            return new ValidationParameterInjector(parameter).Visit(expression is LambdaExpression lambda ? lambda.Body : expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var isClosure =
                node.Type == _parameter.Type &&
                node.Expression.Type.IsClosure();

            return
                isClosure
                    ? _parameter
                    : base.VisitMember(node);
        }
    }
}