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

        public InMemoryResourceProvider(ResourceMetadata metadata)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, true)
                    .Add(ResourceMetadataKeys.CanPut, true)
                    .Add(ResourceMetadataKeys.CanDelete, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            if (uri.IsRelative)
            {
                uri = uri.IsRelative ? (UriString)$"{Scheme}:{uri}" : uri;
            }

            var match = _items.FirstOrDefault(item => item.Uri.Equals(uri));

            return Task.FromResult(match ?? new InMemoryResourceInfo(uri, new byte[0], metadata));
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            var file = new InMemoryResourceInfo(uri, GetByteArray(value), metadata);
            _items.Remove(file);
            _items.Add(file);
            return Task.FromResult<IResourceInfo>(file);
        }

        private byte[] GetByteArray(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public override async Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            var valueToDelete = await GetAsync(uri, metadata);
            _items.Remove(valueToDelete);
            return await GetAsync(uri, metadata);
        }

        public void Add(IResourceInfo item) => _items.Add(item);

        public void Add(string uri, object value)
        {
            var resource = ResourceHelper.CreateStream(value);
            PutAsync(uri, resource.Stream, resource.Metadata).GetAwaiter().GetResult();            
        }

        public IEnumerator<IResourceInfo> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}
