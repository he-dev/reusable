using System.Linq.Expressions;

namespace Reusable.Essentials.Extensions;

public static class ExpressionExtensions
{
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