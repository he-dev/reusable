using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo
{
    public static class ExpressionContextExtensions
    {
        [CanBeNull]
        public static TParameter GetByCallerName<TParameter>(this IExpressionContext context, [CallerMemberName] string itemName = null)
        {
            return (TParameter)context[itemName];
        }
        
        [NotNull]
        public static TExpressionContext SetByCallerName<TParameter, TExpressionContext>(this TExpressionContext context, TParameter value, [CallerMemberName] string itemName = null) 
            where TExpressionContext : IExpressionContext
        {
            context.SetItem(itemName, value);
            return context;
        }

        [NotNull]
        public static IDisposable Scope(this IExpressionContext context, IExpression expression) => ExpressionContextScope.Push(expression, context);

        #region Getters & Setters

        public static TProperty Get<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            Expression<Func<TExpression, TProperty>> propertySelector
        )
        {
            if (context.TryGetValue(CreateKey(item, propertySelector), out var value))
            {
                if (value is TProperty result)
                {
                    return result;
                }
                else
                {
                    throw new ArgumentException
                    (
                        $"There is a value for '{CreateKey(item, propertySelector)}' " +
                        $"but its type '{value.GetType().ToPrettyString()}' " +
                        $"is different from '{typeof(TProperty).ToPrettyString()}'"
                    );
                }
            }
            else
            {
                if (((MemberExpression)propertySelector.Body).Member.IsDefined(typeof(RequiredAttribute)))
                {
                    throw DynamicException.Create("RequiredValueMissing", $"{CreateKey(item, propertySelector)} is required.");
                }

                return default;
            }
        }

        public static IExpressionContext Set<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            Expression<Func<TExpression, TProperty>> selectProperty,
            TProperty value
        )
        {
            return context.SetItem(CreateKey(item, selectProperty), value);
        }

        private static string CreateKey<T, TProperty>
        (
            Item<T> item,
            Expression<Func<T, TProperty>> propertySelector
        )
        {
            if (!(propertySelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException($"'{nameof(propertySelector)}' must be member-expression.");
            }

            return $"{typeof(T).ToPrettyString()}.{memberExpression.Member.Name}";
        }

        #endregion
    }
    
    public class Item<T>
    { }

    public static class Item
    {
        public static Item<T> For<T>() => new Item<T>();
    }
}