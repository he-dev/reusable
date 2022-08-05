using System;
using Reusable.Marbles.Extensions;

namespace Reusable.Data
{
    /// <summary>
    /// This interface provides APIs used by containers. 
    /// </summary>
    /// <remarks>Containers are used to hold data of the same type.</remarks>
    public interface IContainer<in TKey, TValue>
    {
        /// <summary>
        /// Tries to get an item.
        /// </summary>
        Maybe<TValue> GetItem(TKey key);

        /// <summary>
        /// Adds an item or updates the current one.
        /// </summary>
        void AddOrUpdateItem(TKey key, TValue value);

        /// <summary>
        /// Removes an item.
        /// </summary>
        bool RemoveItem(TKey key);
    }
    
    /// <summary>
    /// This container allows only reading and throws NotSupportedException for other APIs.
    /// </summary>
    public abstract class ReadOnlyContainer<TKey, TValue> : IContainer<TKey, TValue>
    {
        public abstract Maybe<TValue> GetItem(TKey key);

        public void AddOrUpdateItem(TKey key, TValue value)
        {
            throw new NotSupportedException($"{typeof(ReadOnlyContainer<TKey, TValue>).ToPrettyString()} does not support adding or updating items.");
        }

        public bool RemoveItem(TKey key)
        {
            throw new NotSupportedException($"{typeof(ReadOnlyContainer<TKey, TValue>).ToPrettyString()} does not support removing items.");
        }
    }

    public static class ContainerExtensions
    {
        public static void AddOrUpdateItem<TKey, TValue>(this IContainer<TKey, TValue> container, Func<TValue, TKey> keySelector, TValue value)
        {
            container.AddOrUpdateItem(keySelector(value), value);
        }
    }
}