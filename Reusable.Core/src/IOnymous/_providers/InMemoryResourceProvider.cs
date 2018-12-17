using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public class InMemoryResourceProvider : ResourceProvider, IEnumerable<IResourceInfo>
    {
        private readonly ISet<IResourceInfo> _items = new HashSet<IResourceInfo>();

        public InMemoryResourceProvider(ResourceProviderMetadata metadata)
            : base(
                (metadata ?? ResourceProviderMetadata.Empty)
                    .Add(ResourceProviderMetadataKeyNames.CanGet, true)
                    .Add(ResourceProviderMetadataKeyNames.CanPut, true)
                    .Add(ResourceProviderMetadataKeyNames.CanDelete, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            if (uri.IsRelative)
            {
                uri = uri.IsRelative ? (SimpleUri) $"{Scheme}:{uri}" : uri;
            }

            var match = _items.FirstOrDefault(item => item.Uri.Equals(uri));
            
            return Task.FromResult(match ?? new InMemoryResourceInfo(uri, default(byte[])));
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            var file = new InMemoryResourceInfo(uri, GetByteArray(value));
            _items.Remove(file);
            _items.Add(file);
            return Task.FromResult<IResourceInfo>(file);

            byte[] GetByteArray(Stream stream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public override async Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            var valueToDelete = await GetAsync(uri, metadata);
            _items.Remove(valueToDelete);
            return await GetAsync(uri, metadata);
        }

        public void Add(IResourceInfo item) => _items.Add(item);

        public void Add(string uri, object value)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, value);
                //memoryStream.Seek(0, SeekOrigin.Begin);
                _items.Add(new InMemoryResourceInfo(uri, memoryStream.ToArray()));
            }
        }

        public IEnumerator<IResourceInfo> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}
