using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.ReMember;

namespace Reusable.Data
{
    public static class ImmutableContainerExtensions
    {
        [NotNull]
        public static IImmutableContainer ThisOrEmpty(this IImmutableContainer? container) => container ?? ImmutableContainer.Empty;

        #region Helpers

        public static bool ContainsKey<T>(this IImmutableContainer container, Selector<T> selector)
        {
            return container.ContainsKey(selector.ToString());
        }

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

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static IImmutableList<T> GetItemOrDefault<T>(this IImmutableContainer container, Selector<IImmutableList<T>> selector, IImmutableList<T>? defaultValue = default)
        {
            return container.TryGetItem(selector, out var item) ? item : defaultValue ?? ImmutableList<T>.Empty;
        }

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static IImmutableSet<T> GetItemOrDefault<T>(this IImmutableContainer container, Selector<IImmutableSet<T>> selector, IImmutableSet<T>? defaultValue = default)
        {
            return container.TryGetItem(selector, out var item) ? item : defaultValue ?? ImmutableHashSet<T>.Empty;
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

            value = default!;
            return false;
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, T value)
        {
            return container.SetItem(key.ToString(), value!);
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, T value, Func<IImmutableContainer, T, bool> predicate)
        {
            return
                predicate(container, value)
                    ? container.SetItem(key.ToString(), value!)
                    : container;
        }

        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<T> key, Func<IImmutableContainer, T> value)
        {
            return container.SetItem(key.ToString(), value(container)!);
        }
        
        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<IEnumerable<T>> key, params T[] items)
        {
            return container.SetItem(key.ToString(), items);
        }
        
        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<IImmutableList<T>> key, params T[] items)
        {
            return container.SetItem(key.ToString(), ImmutableList.CreateRange(items));
        }
        
        public static IImmutableContainer SetItem<T>(this IImmutableContainer container, Selector<IImmutableSet<T>> key, params T[] items)
        {
            return container.SetItem(key.ToString(), ImmutableHashSet.CreateRange(items));
        }

        public static IImmutableContainer Union(this IImmutableContainer first, IImmutableContainer? second)
        {
            return (second ?? ImmutableContainer.Empty).Aggregate(first, (current, next) => current.SetItem(next.Key, next.Value));
        }

        #endregion

        // public static IImmutableContainer UpdateItem<T>(this IImmutableContainer container, Selector<T> key, Func<T, T> update)
        // {
        //     return container.SetItem(key, update(container.GetItemOrDefault(key)));
        // }

        public static IImmutableContainer UpdateItem<T>(this IImmutableContainer container, Selector<IImmutableList<T>> key, Func<IImmutableList<T>, IImmutableList<T>> update)
        {
            return container.SetItem(key, update(container.GetItemOrDefault(key, ImmutableList<T>.Empty)));
        }

        public static IImmutableContainer UpdateItem<T>(this IImmutableContainer container, Selector<IImmutableSet<T>> key, Func<IImmutableSet<T>, IImmutableSet<T>> update, IEqualityComparer<T>? comparer = default)
        {
            return container.SetItem(key, update(container.GetItemOrDefault(key, ImmutableHashSet.Create(comparer ?? EqualityComparer<T>.Default))));
        }

        public static IImmutableContainer UpdateItem<TKey, TValue>
        (
            this IImmutableContainer container,
            Selector<IImmutableDictionary<TKey, TValue>> key,
            Func<IImmutableDictionary<TKey, TValue>, IImmutableDictionary<TKey, TValue>> update,
            IEqualityComparer<TKey>? comparer = default
        )
        {
            return container.SetItem(key, update(container.GetItemOrDefault(key, ImmutableDictionary.Create<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default))));
        }

        public static IImmutableContainer SetWhen(this IImmutableContainer container, Func<IImmutableContainer, bool> canSet, Func<IImmutableContainer, IImmutableContainer> set)
        {
            return
                canSet(container)
                    ? set(container)
                    : container;
        }

        public static IImmutableContainer SetItemWhenNotExists<T>(this IImmutableContainer container, Selector<T> key, T value)
        {
            return
                container.ContainsKey(key.ToString())
                    ? container
                    : container.SetItem(key, value);
        }

        public static IImmutableContainer SetItemWhen<T>
        (
            this IImmutableContainer container,
            Selector<T> key,
            T value,
            Func<IImmutableContainer, bool> predicate
        )
        {
            return
                predicate(container)
                    ? container.SetItem(key, value)
                    : container;
        }

        // Copies items specified by the selectors into a new container.
        public static IImmutableContainer Copy(this IImmutableContainer container, IEnumerable<Selector> selectors)
        {
            var copyable =
                from selector in selectors
                where container.ContainsKey(selector.ToString())
                select selector;

            return copyable.Aggregate(ImmutableContainer.Empty, (current, next) => current.SetItem(next.ToString(), container[next.ToString()]));
        }

        public static IImmutableContainer Copy<T>(this IImmutableContainer container) where T : SelectorBuilder<T>
        {
            // ReSharper disable once PossibleNullReferenceException - This is definitely not-null.
            var selectors = (IEnumerable<Selector>)typeof(SelectorBuilder<T>).GetProperty(nameof(SelectorBuilder<T>.Selectors), BindingFlags.Public | BindingFlags.Static).GetValue(null);
            return container.Copy(selectors);
        }
    }
}