using System;
using System.Linq.Expressions;

namespace Reusable.PropertyMiddleware
{
    internal static class ExpressionExtensions
    {
        public static string GetMemberName(this Expression expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda == null) { throw new ArgumentException("Expression must be a lambda expression."); }
            var memberExpression =
                (lambda.Body as MemberExpression) ??
                (lambda.Body as UnaryExpression)?.Operand as MemberExpression;

            if (memberExpression == null) { throw new ArgumentException("Expression must be a body expression."); }

            return memberExpression.Member.Name;
        }
    }
}