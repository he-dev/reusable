using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public class InMemoryResourceProvider : ResourceProvider, IEnumerable<IResourceInfo>
    {
        private readonly ISet<IResourceInfo> _items = new HashSet<IResourceInfo>();

        public InMemoryResourceProvider(IEnumerable<SoftString> schemes, ResourceMetadata metadata = null)
            : base(schemes, (metadata ?? ResourceMetadata.Empty))
        {
        }

        public InMemoryResourceProvider(ResourceMetadata metadata = null)
            : base(new[] { DefaultScheme }, (metadata ?? ResourceMetadata.Empty))
        {
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var firstMatch = _items.FirstOrDefault(item => item.Uri == uri);
            return Task.FromResult(firstMatch ?? new InMemoryResourceInfo(uri));
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            var resource = new InMemoryResourceInfo(uri, metadata.Format(), value);
            _items.Remove(resource);
            _items.Add(resource);
            return Task.FromResult<IResourceInfo>(resource);
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var resourceToDelete = await GetAsync(uri, metadata);
            _items.Remove(resourceToDelete);
            return await GetAsync(uri, metadata);
        }

        public IEnumerator<IResourceInfo> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }

    public static class InMemoryResourceProviderExtensions
    {
        #region Collection initilizers

        //public void Add(IResourceInfo item) => _items.Add(item);

        public static InMemoryResourceProvider Add(this InMemoryResourceProvider inMemory, string uri, object value)
        {
            switch (value)
            {
                case string str:
                {
                    var stream = ResourceHelper.SerializeAsTextAsync(str).GetAwaiter().GetResult();
                    inMemory.PutAsync(uri, stream, ResourceMetadata.Empty.Format(MimeType.Text)).GetAwaiter().GetResult();
                }

                    break;
                default:
                {
                    var stream = ResourceHelper.SerializeAsBinaryAsync(value).GetAwaiter().GetResult();
                    inMemory.PutAsync(uri, stream, ResourceMetadata.Empty.Format(MimeType.Binary)).GetAwaiter().GetResult();
                }

                    break;
            }

            return inMemory;
        }

        #endregion
    }

    public class InMemoryResourceInfo : ResourceInfo
    {
        [CanBeNull] private readonly Stream _data;

        public InMemoryResourceInfo(UriString uri, MimeType format, Stream data)
            : base(uri, format)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public InMemoryResourceInfo(UriString uri)
            : this(uri, MimeType.Null, Stream.Null)
        {
            ModifiedOn = DateTime.UtcNow;
        }

        public override bool Exists => _data?.Length > 0;

        public override long? Length => _data?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            await _data.Rewind().CopyToAsync(stream);
        }
    }
}