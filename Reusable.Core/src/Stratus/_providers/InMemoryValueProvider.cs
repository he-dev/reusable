using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Reusable.Stratus
{
    using static ValueProviderMetadataKeyNames;

    public class InMemoryValueProvider : ValueProvider, IEnumerable<IValueInfo>
    {
        private readonly ISet<IValueInfo> _items = new HashSet<IValueInfo>();

        public InMemoryValueProvider(ValueProviderMetadata metadata)
            : base(
                (metadata ?? ValueProviderMetadata.Empty)
                    .Add(CanDeserialize, true)
                    .Add(CanSerialize, true)
                    .Add(CanDelete, true)
            )
        { }

        public override Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            var file = _items.SingleOrDefault(f => ValueInfoEqualityComparer.Default.Equals(f.Name, name));
            return Task.FromResult<IValueInfo>(file ?? new InMemoryValueInfo(name, default(byte[])));
        }

        public override Task<IValueInfo> SerializeAsync(string path, Stream value, ValueProviderMetadata metadata = null)
        {
            var file = new InMemoryValueInfo(path, GetByteArray(value));
            _items.Remove(file);
            _items.Add(file);
            return Task.FromResult<IValueInfo>(file);

            byte[] GetByteArray(Stream stream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
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
            var valueToDelete = await GetValueInfoAsync(name, metadata);
            _items.Remove(valueToDelete);
            return await GetValueInfoAsync(name, metadata);
        }

        public void Add(IValueInfo item) => _items.Add(item);

        public IEnumerator<IValueInfo> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}
