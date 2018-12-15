using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.Stratus
{
    using static ValueProviderMetadataKeyNames;

    public class CompositeValueProvider : ValueProvider, IEnumerable<IValueProvider>
    {
        private readonly Dictionary<SoftString, IValueProvider> _valueProviderCache;

        private readonly SemaphoreSlim _valueProviderCacheLock = new SemaphoreSlim(1, 1);

        private readonly IImmutableList<IValueProvider> _valueProviders;

        public CompositeValueProvider
        (
            IImmutableList<IValueProvider> valueProviders,
            ValueProviderMetadata metadata
        )
            : base(
                metadata
                    .Add(CanDeserialize, valueProviders.Any(x => x.Metadata.ContainsKey(CanDeserialize)))
                    .Add(CanSerialize, valueProviders.Any(x => x.Metadata.ContainsKey(CanSerialize)))
                    .Add(CanDelete, valueProviders.Any(x => x.Metadata.ContainsKey(CanDelete)))
            )
        {
            _valueProviderCache = new Dictionary<SoftString, IValueProvider>();
            _valueProviders = valueProviders;
        }

        public override async Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            await _valueProviderCacheLock.WaitAsync();
            try
            {
                // In provider-name specified then try to get the value from this provider without using caching.
                if (metadata.TryGetValue(ProviderName, out string providerNameToFind))
                {
                    var valueProvider =
                        _valueProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ProviderName, out string providerName)
                                && providerName == providerNameToFind
                            );

                    if (!(valueProvider is null))
                    {
                        return await valueProvider.GetValueInfoAsync(name, metadata);
                    }
                }
                else
                {
                    // Use either the cached value-provider of find a new one.

                    if (_valueProviderCache.TryGetValue(name, out var cachedValueProvider))
                    {
                        return await cachedValueProvider.GetValueInfoAsync(name, metadata);
                    }
                    else
                    {
                        foreach (var valueProvider in _valueProviders)
                        {
                            var value = await valueProvider.GetValueInfoAsync(name, metadata);
                            if (value.Exists)
                            {
                                _valueProviderCache[name] = valueProvider;
                                return value;
                            }
                        }
                    }
                }
            }
            finally
            {
                _valueProviderCacheLock.Release();
            }

            return new InMemoryValueInfo(name);
        }

        public override async Task<IValueInfo> SerializeAsync(string name, Stream data, ValueProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(name, metadata);

            if (!valueProvider.Metadata.TryGetValue(CanSerialize, out bool _))
            {
                throw DynamicException.Create("SerializeNotSupported", $"Value-provider '{valueProvider.GetType().ToPrettyString()}' doesn't support '{nameof(SerializeAsync)}'.");
            }

            return await valueProvider.SerializeAsync(name, data, metadata);

        }

        public override async Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, value);
                return await SerializeAsync(name, memoryStream, metadata);
            }
        }

        public override async Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            var valueProvider = await GetValueProviderAsync(name, metadata);

            if (!valueProvider.Metadata.TryGetValue(CanDelete, out bool _))
            {
                throw DynamicException.Create("DeleteNotSupported", $"Value-provider '{valueProvider.GetType().ToPrettyString()}' doesn't support '{nameof(DeleteAsync)}'.");
            }

            return await valueProvider.DeleteAsync(name, metadata);
        }

        [ItemNotNull]
        private async Task<IValueProvider> GetValueProviderAsync(string name, ValueProviderMetadata metadata = null)
        {
            await _valueProviderCacheLock.WaitAsync();
            try
            {
                if (metadata.TryGetValue(ProviderName, out string providerNameToFind))
                {
                    return
                        _valueProviders
                            .SingleOrDefault(p =>
                                p.Metadata.TryGetValue(ProviderName, out string providerName)
                                && providerName == providerNameToFind
                            );
                }

                if (_valueProviderCache.TryGetValue(name, out var cachedValueProvider))
                {
                    return cachedValueProvider;
                }

                throw DynamicException.Create
                (
                    "UnknownValueProvider",
                    $"Could not serialize '{name}' because serializing requires a well-known-value-provider and it could be determined. " +
                    $"This means that it needs to be either specified via '{nameof(metadata)}' or be already determined by calling '{nameof(GetValueInfoAsync)}'."
                );
            }
            finally
            {
                _valueProviderCacheLock.Release();
            }
        }

        public IEnumerator<IValueProvider> GetEnumerator() => _valueProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_valueProviders).GetEnumerator();
    }
}