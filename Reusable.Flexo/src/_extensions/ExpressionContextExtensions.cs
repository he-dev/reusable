using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    [PublicAPI]
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

//        [NotNull]
//        public static TProperty Get<TExpression, TProperty>
//        (
//            this IImmutableSession context,
//            Item<TExpression> item,
//            linq.Expression<Func<TExpression, TProperty>> propertySelector
//        )
//        {
//            if (context.TryGetValue(ExpressionContext.CreateKey(item, propertySelector), out var value))
//            {
//                if (value is TProperty result)
//                {
//                    return result;
//                }
//                else
//                {
//                    throw new ArgumentException
//                    (
//                        $"There is a value for '{ExpressionContext.CreateKey(item, propertySelector)}' " +
//                        $"but its type '{value.GetType().ToPrettyString()}' " +
//                        $"is different from '{typeof(TProperty).ToPrettyString()}'"
//                    );
//                }
//            }
//            else
//            {
//                //if (((MemberExpression)propertySelector.Body).Member.IsDefined(typeof(RequiredAttribute)))
//                {
//                    throw DynamicException.Create("RequiredValueMissing", $"{ExpressionContext.CreateKey(item, propertySelector)} is required.");
//                }
//
//                //throw DynamicException.Create("");
//            }
//        }
//
//        public static IExpressionSession Set<TExpression, TProperty>
//        (
//            this IImmutableSession context,
//            Item<TExpression> item,
//            linq.Expression<Func<TExpression, TProperty>> selectProperty,
//            TProperty value
//        )
//        {
//            var key = ExpressionContext.CreateKey(item, selectProperty);
//            return context.SetItem(key, value); // is IConstant constant ? constant : Constant.FromValue(key, value));
//        }

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

        #region Extension-Input helpers

        public static IImmutableSession PushExtensionInput(this IImmutableSession context, object value)
        {
            context.Get(Use<IExpressionSession>.Scope, x => x.ExtensionInputs).Push(value);
            return context;
        }

        // The Input must be removed so that subsequent expression doesn't 'think' it's called as extension when it isn't.
        public static bool TryPopExtensionInput<T>(this IImmutableSession context, out T input)
        {
            var inputs = context.Get(Use<IExpressionSession>.Scope, x => x.ExtensionInputs);
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

        public static IImmutableSession WithComparer(this IImmutableSession context, string name, IEqualityComparer<object> comparer)
        {
            context.Get(Use<IExpressionSession>.Scope, x => x.Comparers).AddSafely(name, comparer);
            return context;
        }

        public static IImmutableSession WithSoftStringComparer(this IImmutableSession context)
        {
            return context.WithComparer("SoftString", EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            ));
        }

        public static IImmutableSession WithRegexComparer(this IImmutableSession context)
        {
            return context.WithComparer("Regex", EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => Regex.IsMatch((string)right, (string)left, RegexOptions.None),
                getHashCode: (obj) => 0
            ));
        }        

        public static IEqualityComparer<object> GetComparerOrDefault(this IImmutableSession context, string name)
        {
            var comparers = context.Get(Use<IExpressionSession>.Scope, x => x.Comparers);
            if (name is null)
            {
                return comparers["Default"];
            }

            if (comparers.TryGetValue(name, out var comparer))
            {
                return comparer;
            }

            throw DynamicException.Create("ComparerNotFound", $"There is no comparer with the name '{name}'.");
        }

        public static TreeNode<ExpressionDebugView> DebugView(this IImmutableSession context)
        {
            return context.Get(Use<IExpressionSession>.Scope, x => x.DebugView);
        }
        
        public static IImmutableSession DebugView(this IImmutableSession context, TreeNode<ExpressionDebugView> debugView)
        {
            return context.Set(Use<IExpressionSession>.Scope, x => x.DebugView, debugView);
        }
    }

//    public class Item<T>
//    { }
//
//    public static class Item
//    {
//        public static Item<T> For<T>() => new Item<T>();
//    }
}