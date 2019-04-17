using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OneTo1;

namespace Reusable.IOnymous
{
    public class InMemoryProvider : ResourceProvider, IEnumerable<KeyValuePair<SoftString, object>>
    {
        private readonly ITypeConverter<UriString, string> _uriConverter;
        private readonly IDictionary<SoftString, object> _items = new Dictionary<SoftString, object>();

        public InMemoryProvider(IEnumerable<SoftString> schemes, ResourceMetadata metadata = default)
            : base(schemes, metadata)
        { }

        public InMemoryProvider(ResourceMetadata metadata = default)
            : base(new[] { DefaultScheme }, metadata)
        { }

        public InMemoryProvider(ITypeConverter<UriString, string> uriConverter, IEnumerable<SoftString> schemes, ResourceMetadata metadata = default)
            : this(schemes, metadata)
        {
            _uriConverter = uriConverter;
        }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            var name = _uriConverter.Convert<string>(uri);
            return
                _items.TryGetValue(name, out var o)
                    // todo - hardcoded string format
                    ? new InMemoryResourceInfo(uri, MimeType.Text, await ResourceHelper.SerializeTextAsync((string)o))
                    : new InMemoryResourceInfo(uri);
        }

        // protected override Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata)
        // {
        //     ValidateFormatNotNull(this, uri, metadata);
        //
        //     //var resource = new InMemoryResourceInfo(uri, metadata.Format(), value);
        //     //_items.Remove(resource);
        //     //_items.Add(resource);
        //     //return Task.FromResult<IResourceInfo>(resource);
        // }

        // protected override Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata)
        // {
        //     ValidateFormatNotNull(this, uri, metadata);
        //
        //     var resource = new InMemoryResourceInfo(uri, metadata.Format(), value);
        //     _items.Remove(resource);
        //     _items.Add(resource);
        //     return Task.FromResult<IResourceInfo>(resource);
        // }

        // protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata)
        // {
        //     var resourceToDelete = await GetAsync(uri, metadata);
        //     _items.Remove(resourceToDelete);
        //     return await GetAsync(uri, metadata);
        // }

        #region Collection initilizers

        //public void Add(IResourceInfo item) => _items.Add(item);

        public void Add(string name, object value)
        {
            _items[name] = value;
        }

        #endregion

        public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }

    public static class InMemoryProviderExtensions
    {
        #region Collection initilizers

        //public void Add(IResourceInfo item) => _items.Add(item);

        public static InMemoryProvider Add(this InMemoryProvider inMemory, string uri, object value)
        {
            switch (value)
            {
                case string str:
                {
                    var stream = ResourceHelper.SerializeTextAsync(str).GetAwaiter().GetResult();
                    inMemory.PutAsync(uri, stream, ResourceMetadata.Empty.Format(MimeType.Text)).GetAwaiter().GetResult();
                }

                    break;
                default:
                {
                    var stream = ResourceHelper.SerializeBinaryAsync(value).GetAwaiter().GetResult();
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
        [CanBeNull]
        private readonly Stream _data;

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