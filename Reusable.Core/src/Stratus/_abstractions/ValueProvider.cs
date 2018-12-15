using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    [PublicAPI]
    public interface IValueProvider
    {
        [NotNull]
        ValueProviderMetadata Metadata { get; }

        [ItemNotNull]
        Task<IValueInfo> GetValueInfoAsync([NotNull] string name, ValueProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IValueInfo> SerializeAsync([NotNull] string name, [NotNull] Stream value, ValueProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IValueInfo> SerializeAsync([NotNull] string name, [NotNull] object value, ValueProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IValueInfo> DeleteAsync([NotNull] string name, ValueProviderMetadata metadata = null);
    }

    public abstract class ValueProvider : IValueProvider
    {
        protected ValueProvider(ValueProviderMetadata metadata)
        {
            Metadata = metadata;
        }

        public virtual ValueProviderMetadata Metadata { get; }

        public abstract Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null);

        public abstract Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null);

        public abstract Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null);

        public abstract Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null);

        protected static Exception CreateException(IValueProvider provider, string name, ValueProviderMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            return new Exception();
        }
    }

    //[Flags]
    //public enum ValueProviderCapabilities
    //{
    //    None = 0,

    //    CanReadValue = 1 << 1,
    //    CanWriteValue = 1 << 2,
    //}

    public class ValueProviderMetadata
    {
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        public ValueProviderMetadata() : this(ImmutableDictionary<SoftString, object>.Empty) { }

        private ValueProviderMetadata(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static ValueProviderMetadata Empty => new ValueProviderMetadata();

        public object this[SoftString key] => _metadata[key];
        public int Count => _metadata.Count;
        public IEnumerable<SoftString> Keys => _metadata.Keys;
        public IEnumerable<object> Values => _metadata.Values;
        public bool ContainsKey(SoftString key) => _metadata.ContainsKey(key);
        public bool Contains(KeyValuePair<SoftString, object> pair) => _metadata.Contains(pair);
        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => _metadata.TryGetKey(equalKey, out actualKey);
        public bool TryGetValue(SoftString key, out object value) => _metadata.TryGetValue(key, out value);

        public ValueProviderMetadata Add(SoftString key, object value) => new ValueProviderMetadata(_metadata.Add(key, value));
        //public IImmutableDictionary<SoftString, object> Clear() => new ValueProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ValueProviderMetadata(_metadata.AddRange(pairs));
        //public IImmutableDictionary<SoftString, object> SetItem(SoftString key, object value) => new ValueProviderMetadata(_metadata.SetItem(key, value));
        //public IImmutableDictionary<SoftString, object> SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ValueProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ValueProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ValueProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }

    public static class ValueProviderMetadataExtensions
    {
        public static bool TryGetValue<T>(this ValueProviderMetadata metadata, SoftString key, out T value)
        {
            if (metadata.TryGetValue(key, out var x) && x is T result)
            {
                value = result;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public static class ValueProviderMetadataKeyNames
    {
        public static string ProviderName { get; } = nameof(ProviderName);

        public static string CanSerialize { get; } = nameof(CanSerialize);

        public static string CanDeserialize { get; } = nameof(CanDeserialize);

        public static string CanDelete { get; } = nameof(CanDelete);
    }
}
