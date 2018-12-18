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
        {
        }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);

            var firstMatch = _items.FirstOrDefault(item => item.Uri == uri);
            return Task.FromResult(firstMatch ?? new InMemoryResourceInfo(uri, metadata));
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);

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
            ValidateSchemeNotEmpty(uri);

            var resourceToDelete = await GetAsync(uri, metadata);
            _items.Remove(resourceToDelete);
            return await GetAsync(uri, metadata);
        }

        #region Collection initilizer

        public void Add(IResourceInfo item) => _items.Add(item);

        public void Add(string uri, object value)
        {
            var (stream, metadata) = ResourceHelper.CreateStream(value);
            PutAsync(uri, stream, metadata).GetAwaiter().GetResult();
        }

        #endregion

        public IEnumerator<IResourceInfo> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}