using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.Data
{
    public static class ImmutableContainerExtensions
    {
        [NotNull]
        public static IImmutableContainer ThisOrEmpty(this IImmutableContainer container) => container ?? ImmutableContainer.Empty;

        #region Helpers

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static T GetItem<T>(this IImmutableContainer container, Selector<T> selector)
        {
            return container.TryGetItem(selector, out var item) ? item : throw DynamicException.Create("ItemNotFound", $"Item '{selector}' is required.");
        }

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static T GetItemOrDefault<T>(this IImmutableContainer container, Selector<T> selector, T defaultValue = default)
        {
            return container.TryGetItem(selector, out var item) ? item : defaultValue;
        }

        public static bool TryGetItem<T>(this IImmutableContainer container, Selector<T> selector, out T value)
        {
            if (container.TryGetValue(selector.ToString(), out var item))
            {
                if (item is T t)
                {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, T value)
        {
            return container.SetItem(key.ToString(), value);
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, T value, Func<IImmutableContainer, T, bool> predicate)
        {
            return
                predicate(container, value)
                    ? container.SetItem(key.ToString(), value)
                    : container;
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, Func<IImmutableContainer, T> value)
        {
            return container.SetItem(key.ToString(), value(container));
        }

        public static Func<IImmutableContainer, IImmutableContainer> MergeFunc(IImmutableContainer other)
        {
            return current =>
            {
                foreach (var (key, value) in other)
                {
                    current = current.SetItem(key, value);
                }

                return current;
            };
        }

        public static IImmutableContainer Union(this IImmutableContainer first, IImmutableContainer second, bool overwrite = false)
        {
            return second.Aggregate(first, (current, next) => overwrite || !current.ContainsKey(next.Key) ? current.SetItem(next.Key, next.Value) : current);
        }

        //        [MustUseReturnValue]
        //        public static IImmutableSession Union(this IImmutableSession metadata, IImmutableSession other)
        //        {
        //            //return other.Aggregate(metadata, (current, i) => current.SetItem(i.Key, i.Value));
        //
        //            var result = metadata;
        //
        //            foreach (var item in other)
        //            {
        //                if (item.Value is IImmutableSession otherScope)
        //                {
        //                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        //                    if (result.TryGetValue(item.Key, out IImmutableSession currentScope))
        //                    {
        //                        result = result.SetItem(item.Key, currentScope.Aggregate(otherScope, (current, i) => current.SetItem(i.Key, i.Value)));
        //                    }
        //                    else
        //                    {
        //                        result = result.SetItem(item.Key, otherScope);
        //                    }
        //                }
        //                else
        //                {
        //                    result = result.SetItem(item.Key, item.Value);
        //                }
        //            }
        //
        //            return result;
        //        }

        //        public static MetadataScope<T> Union<T>(this MetadataScope<T> scope, Metadata other)
        //        {
        //            return scope.Metadata.Union(other);
        //        }

        #endregion

        public static IImmutableContainer SetWhen(this IImmutableContainer container, Func<IImmutableContainer, bool> canSet, Func<IImmutableContainer, IImmutableContainer> set)
        {
            return
                canSet(container)
                    ? set(container)
                    : container;
        }
    }
}