using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Reusable.Synergy.Expressions;

namespace Reusable.Synergy;

public interface IPropertyAccessor<TValue>
{
    Func<object, TValue> GetValue { get; }

    Action<object, TValue> SetValue { get; }
}

public record PropertyAccessor<TValue>(Func<object, TValue> GetValue, Action<object, TValue> SetValue) : IPropertyAccessor<TValue>;

public static class PropertyService
{
    private static readonly ConcurrentDictionary<(Type, string), object> Cache = new();

    public abstract class For<TSource> where TSource : IRequest
    {
        public static IPropertyAccessor<TValue> Select<TValue>(Expression<Func<TSource, TValue>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}");
            }

            return (IPropertyAccessor<TValue>)Cache.GetOrAdd((typeof(TSource), memberExpression.Member.Name), _ =>
            {
                var targetParameter = Expression.Parameter(typeof(object), "target");
                var valueParameter = Expression.Parameter(typeof(TValue), "value");

                var casted = ParameterConverter<TSource>.Rewrite(memberExpression, targetParameter);

                // ((T)target).Property
                var getter =
                    Expression.Lambda<Func<object, TValue>>(
                        casted,
                        targetParameter
                    ).Compile();

                // ((T)target).Property = value
                var setter =
                    Expression.Lambda<Action<object, TValue>>(
                        Expression.Assign(casted, valueParameter),
                        targetParameter, valueParameter
                    ).Compile();

                return new PropertyAccessor<TValue>(getter, setter);
            });
        }
    }
}