using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Reusable.Stratus
{
    using static ValueProviderMetadataKeyNames;

    public class CompositeValueProvider : ValueProvider, IEnumerable<IValueProvider>
    {
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
                )
        {
            _valueProviders = valueProviders;
        }

        public override async Task<IValueInfo> GetValueInfoAsync(string path, ValueProviderMetadata metadata = null)
        {
            foreach (var fileProvider in _valueProviders)
            {
                var fileInfo = await fileProvider.GetValueInfoAsync(path);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }

            return new InMemoryValueInfo(path, new byte[0]);
        }

        public override Task<IValueInfo> SerializeAsync(string path, Stream data, ValueProviderMetadata metadata = null)
        {
            //_fileProviders.Where(p => p.Metadata.TryGetValue(CanSerialize, out var flag) && flag is bool canSerialize && canSerialize))
            {

            }
            throw new NotSupportedException($"{nameof(CompositeValueProvider)} does not support file creation.");
        }

        public override Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            throw new NotImplementedException();
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IValueProvider> GetEnumerator() => _valueProviders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_valueProviders).GetEnumerator();

    }

    public static class CompositeValueProviderMetadata
    {
        public static string Name { get; } = nameof(Name);

        public static string CanSerialize { get; } = nameof(CanSerialize);

        public static string CanDerialize { get; } = nameof(CanDerialize);
    }
}