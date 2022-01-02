using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Reusable.Synergy.Expressions;

namespace Reusable.Synergy;

public interface IConditionService
{
    Func<object, bool> Evaluate { get; }
}

public record ConditionService(Func<object, bool> Evaluate) : IConditionService
{
    private static readonly ConcurrentDictionary<(Type, string), object> Cache = new();

    public abstract class For<TSource>
    {
        public static IConditionService When(Expression<Func<TSource, bool>> expression, string tag)
        {
            return (IConditionService)Cache.GetOrAdd((typeof(TSource), tag), _ =>
            {
                var targetParameter = Expression.Parameter(typeof(object), "target");

                var casted = ParameterConverter<TSource>.Rewrite(expression.Body, targetParameter);

                // ((T)target)() -> bool
                var evaluate =
                    Expression.Lambda<Func<object, bool>>(
                        casted,
                        targetParameter
                    ).Compile();

                return new ConditionService(evaluate);
            });
        }
    }
}

public record ConditionBag<T>(Func<object, bool> Evaluate, Func<T> GetValue) : IConditionService;

public static class ConditionBagFactory
{
    public static ConditionBag<T> Then<T>(this IConditionService condition, T value)
    {
        return new ConditionBag<T>(condition.Evaluate, () => value);
    }
}