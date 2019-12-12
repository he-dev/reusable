using System;
using Reusable.Extensions;

namespace Reusable.Data
{
    /// <summary>
    /// This interface provides APIs used by containers. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
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
}