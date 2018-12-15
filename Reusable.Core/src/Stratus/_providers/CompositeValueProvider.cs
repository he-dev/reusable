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
                    .Add(CanDelete, valueProviders.Any(x => x.Metadata.ContainsKey(CanDelete)))
            )
        {
            _valueProviders = valueProviders;
        }

        public override async Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            foreach (var fileProvider in _valueProviders)
            {
                var info = await fileProvider.GetValueInfoAsync(name);
                if (info.Exists)
                {
                    return info;
                }
            }

            return new InMemoryValueInfo(name, new byte[0]);
        }

        public override Task<IValueInfo> SerializeAsync(string name, Stream data, ValueProviderMetadata metadata = null)
        {
            var serializers = _valueProviders.Where(p => p.Metadata.TryGetValue(CanSerialize, out var flag) && flag is bool canSerialize && canSerialize);
            

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
}