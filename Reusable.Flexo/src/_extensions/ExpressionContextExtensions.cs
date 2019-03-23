using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Schema;
using Reusable.Exceptionize;
using Reusable.Extensions;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    public static class ExpressionContextExtensions
    {
//        [CanBeNull]
//        public static TParameter GetByCallerName<TParameter>(this IExpressionContext context, [CallerMemberName] string itemName = null)
//        {
//            return (TParameter)context[itemName];
//        }
//
//        [NotNull]
//        public static TExpressionContext SetByCallerName<TParameter, TExpressionContext>(this TExpressionContext context, TParameter value, [CallerMemberName] string itemName = null)
//            where TExpressionContext : IExpressionContext
//        {
//            context.SetItem(itemName, value);
//            return context;
//        }

        //[NotNull]
        //public static IDisposable Scope(this IExpressionContext context, IExpression expression) => ExpressionContextScope.Push(expression, context);

        #region Getters & Setters

        [NotNull]
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
                //if (((MemberExpression)propertySelector.Body).Member.IsDefined(typeof(RequiredAttribute)))
                {
                    throw DynamicException.Create("RequiredValueMissing", $"{ExpressionContext.CreateKey(item, propertySelector)} is required.");
                }

                //throw DynamicException.Create("");
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
            var key = ExpressionContext.CreateKey(item, selectProperty);
            return context.SetItem(key, value); // is IConstant constant ? constant : Constant.FromValue(key, value));
        }

//        public static IExpressionContext SetItem
//        (
//            this IExpressionContext context,
//            string key,
//            object value
//        )
//        {
//            return context.SetItem(key, value is IConstant constant ? constant : Constant.FromValue(key, value));
//        }

        #endregion

        #region Helpers

        public static IExpressionContext PushExtensionInput(this IExpressionContext context, object value)
        {
            context.Get(Item.For<IExtensionContext>(), x => x.Inputs).Push(value);
            return context;
        }

        // The Input must be removed so that subsequent expression doesn't 'think' it's called as extension when it isn't.
        public static bool TryPopExtensionInput<T>(this IExpressionContext context, out T input)
        {
            var inputs = context.Get(Item.For<IExtensionContext>(), x => x.Inputs);
            if (inputs.Any())
            {
                input = (T)inputs.Pop();
                return true;
            }
            else
            {
                input = default;
                return false;
            }
        }

        #endregion
    }

    public class Item<T> { }

    public static class Item
    {
        public static Item<T> For<T>() => new Item<T>();
    }
}