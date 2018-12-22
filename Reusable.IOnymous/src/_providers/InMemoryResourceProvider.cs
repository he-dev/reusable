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
            : base(new SoftString[] { DefaultScheme }, (metadata ?? ResourceMetadata.Empty))
        {
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var firstMatch = _items.FirstOrDefault(item => item.Uri == uri);
            return Task.FromResult(firstMatch ?? new InMemoryResourceInfo(uri, metadata));
        }

        protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            var resource = new InMemoryResourceInfo(uri, GetByteArray(value), metadata);
            _items.Remove(resource);
            _items.Add(resource);
            return Task.FromResult<IResourceInfo>(resource);
        }

        private byte[] GetByteArray(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
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

    public class InMemoryResourceInfo : ResourceInfo
    {
        [CanBeNull] private readonly byte[] _data;

        private readonly ResourceMetadata _metadata;

        [CanBeNull] private readonly IEnumerable<IResourceInfo> _files;

        public InMemoryResourceInfo([NotNull] UriString uri, [NotNull] byte[] data, [CanBeNull] ResourceMetadata metadata = null)
            : base(uri)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _metadata = metadata;
        }

        public InMemoryResourceInfo([NotNull] UriString uri)
            : this(uri, new byte[0], ResourceMetadata.Empty)
        {
            ModifiedOn = DateTime.UtcNow;
        }

        public InMemoryResourceInfo([NotNull] UriString uri, ResourceMetadata metadata)
            : this(uri, new byte[0], metadata)
        {
        }


        public override bool Exists => _data?.Length > 0;

        public override long? Length => _data?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            await stream.WriteAsync(_data, 0, _data.Length);
        }

        protected override Task<object> DeserializeAsyncInternal(Type targetType)
        {
            using (var memoryStream = new MemoryStream(_data))
            {
                return Task.FromResult(ResourceHelper.CreateObject(memoryStream, _metadata));
            }
        }
    }
}