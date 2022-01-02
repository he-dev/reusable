using System.Linq.Expressions;

namespace Reusable.Synergy.Expressions;

// Casts the target object to T and exchanges the parameter for an 'object'.
public class ParameterConverter<T> : ExpressionVisitor
{
    private ParameterExpression ObjectParameter { get; init; } = null!;

    public static Expression Rewrite(Expression expression, ParameterExpression parameter)
    {
        return new ParameterConverter<T> { ObjectParameter = parameter }.Visit(expression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return Expression.Convert(ObjectParameter, typeof(T));
    }
}