using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Reusable.Flawless
{
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
                node.Expression.Type.Name.StartsWith("<>c__DisplayClass") &&
                node.Expression.Type.IsDefined(typeof(CompilerGeneratedAttribute));

            return
                isClosure
                    ? _parameter
                    : base.VisitMember(node);
        }
    }
}