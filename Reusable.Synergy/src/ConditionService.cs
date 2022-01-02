using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Reusable.Synergy.Expressions;

namespace Reusable.Synergy;

public interface ICondition
{
    Func<object, bool> Evaluate { get; }
}

public record Condition(Func<object, bool> Evaluate) : ICondition
{
    private static readonly ConcurrentDictionary<(Type, string), object> Cache = new();

    public abstract class For<TSource> where TSource : IRequest
    {
        public static ICondition When(Expression<Func<TSource, bool>> expression, string tag)
        {
            return (ICondition)Cache.GetOrAdd((typeof(TSource), tag), _ =>
            {
                var targetParameter = Expression.Parameter(typeof(object), "target");

                var casted = ParameterConverter<TSource>.Rewrite(expression.Body, targetParameter);

                // ((T)target)() -> bool
                var evaluate =
                    Expression.Lambda<Func<object, bool>>(
                        casted,
                        targetParameter
                    ).Compile();

                return new Condition(evaluate);
            });
        }
    }
}

public record ConditionBag<T>(Func<object, bool> Evaluate, Func<T> GetValue) : ICondition;

public static class ConditionBagFactory
{
    public static ConditionBag<T> Then<T>(this ICondition condition, T value)
    {
        return new ConditionBag<T>(condition.Evaluate, () => value);
    }
}