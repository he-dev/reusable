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
            return container.TryGetItem(selector, out var item) ? item : throw DynamicException.Create("ItemNotFound", $"There is no item with the key '{selector}'.");
        }

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static T GetItemOrDefault<T>(this IImmutableContainer container, Selector<T> selector, T defaultValue = default)
        {
            return container.TryGetItem(selector, out var item) ? item : defaultValue;
        }

        public static bool TryGetItem<T>(this IImmutableContainer container, Selector<T> selector, out T value)
        {
            if (container.TryGetItem(selector.ToString(), out var item))
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

        public static IImmutableContainer Union(this IImmutableContainer first, IImmutableContainer second)
        {
            return second.Aggregate(first, (current, next) => current.SetItem(next.Key, next.Value));
        }

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