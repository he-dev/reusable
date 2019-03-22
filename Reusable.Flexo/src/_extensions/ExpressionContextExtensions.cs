using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using linq = System.Linq.Expressions;

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

        //[NotNull]
        //public static IDisposable Scope(this IExpressionContext context, IExpression expression) => ExpressionContextScope.Push(expression, context);

        #region Getters & Setters
        
        [CanBeNull]
        public static TProperty Get<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            linq.Expression<Func<TExpression, TProperty>> propertySelector
        )
        {
            if (context.TryGetValue(ExpressionContext.CreateKey(item, propertySelector), out var value))
            {
                if (value is TProperty result)
                {
                    return result;
                }
                else
                {
                    throw new ArgumentException
                    (
                        $"There is a value for '{ExpressionContext.CreateKey(item, propertySelector)}' " +
                        $"but its type '{value.GetType().ToPrettyString()}' " +
                        $"is different from '{typeof(TProperty).ToPrettyString()}'"
                    );
                }
            }
            else
            {
                if (((MemberExpression)propertySelector.Body).Member.IsDefined(typeof(RequiredAttribute)))
                {
                    throw DynamicException.Create("RequiredValueMissing", $"{ExpressionContext.CreateKey(item, propertySelector)} is required.");
                }

                return default;
            }
        }

        public static IExpressionContext Set<TExpression, TProperty>
        (
            this IExpressionContext context,
            Item<TExpression> item,
            linq.Expression<Func<TExpression, TProperty>> selectProperty,
            TProperty value
        )
        {
            return context.SetItem(ExpressionContext.CreateKey(item, selectProperty), value);
        }
        
        #endregion

        #region Helpers

        public static IExpressionContext ExtensionInput(this IExpressionContext context, IExpression value)
        {
            return context.Set(Item.For<IExtensionContext>(), x => x.Input, value);
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