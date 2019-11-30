using System;
using Reusable.Extensions;

namespace Reusable.Data
{
    public interface IContainer<in TKey, TValue>
    {
        Maybe<TValue> GetItem(TKey key);

        void AddOrUpdateItem(TKey key, TValue value);

        bool RemoveItem(TKey key);
    }
    
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