using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Extensions
{
    public static class ExpressionExtensions
    {
        [NotNull]
        public static MemberExpression ToMemberExpression(this LambdaExpression lambdaExpression)
        {
            return
                lambdaExpression.Body is MemberExpression memberExpression
                    ? memberExpression
                    : throw DynamicException.Create
                    (
                        $"NotMemberExpression",
                        $"Expression '{lambdaExpression}' is not a member-expression."
                    );
        }
    }
}