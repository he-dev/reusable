using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Reusable.Essentials;

public static class BinaryOperation<T>
{
    public static T Add(T left, T right) => BinaryOperation.GetOrAdd<T, T>(Expression.Add)(left, right);
    public static T Subtract(T left, T right) => BinaryOperation.GetOrAdd<T, T>(Expression.Subtract)(left, right);
    public static T Multiply(T left, T right) => BinaryOperation.GetOrAdd<T, T>(Expression.Multiply)(left, right);
    public static T Divide(T left, T right) => BinaryOperation.GetOrAdd<T, T>(Expression.Divide)(left, right);

    public static bool Equal(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.Equal)(left, right);
    public static bool NotEqual(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.NotEqual)(left, right);

    public static bool LessThan(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.LessThan)(left, right);
    public static bool LessThanOrEqual(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.LessThanOrEqual)(left, right);

    public static bool GreaterThan(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.GreaterThan)(left, right);

    public static bool GreaterThanOrEqual(T left, T right) => BinaryOperation.GetOrAdd<T, bool>(Expression.GreaterThanOrEqual)(left, right);
        
    //public static BinaryExpression GetExpression() => 
}

public static class BinaryOperation
{
    private static readonly ConcurrentDictionary<Type, (BinaryExpression BinaryExpression, object Func)> Cache = new();

    internal static Func<T, T, TResult> GetOrAdd<T, TResult>(Func<Expression, Expression, BinaryExpression> expressionFunc)
    {
        return (Func<T, T, TResult>)Cache.GetOrAdd(typeof(T), t => CreateBinaryFunc<T, TResult>(expressionFunc)).Func;
    }

    private static (BinaryExpression BinaryExpression, Func<T, T, TResult> Func) CreateBinaryFunc<T, TResult>(Func<Expression, Expression, BinaryExpression> binaryExpression)
    {
        var leftParameter = Expression.Parameter(typeof(T), "left");
        var rightParameter = Expression.Parameter(typeof(T), "right");
        var binaryOperation = binaryExpression(leftParameter, rightParameter);
        return
        (
            binaryOperation,
            Expression.Lambda<Func<T, T, TResult>>(binaryOperation, leftParameter, rightParameter).Compile()
        );
    }
}