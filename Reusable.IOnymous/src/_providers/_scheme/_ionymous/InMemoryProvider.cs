using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class InMemoryProvider : ResourceProvider, IEnumerable<KeyValuePair<SoftString, object>>
    {
        private readonly ITypeConverter<UriString, string> _uriConverter;
        private readonly IDictionary<SoftString, object> _items = new Dictionary<SoftString, object>();

        //public InMemoryProvider(IEnumerable<SoftString> schemes, Metadata metadata = default) : base(schemes, metadata) { }

        //public InMemoryProvider(Metadata metadata = default) : base(new[] { DefaultScheme }, metadata) { }

        public InMemoryProvider([NotNull] ITypeConverter<UriString, string> uriConverter, IImmutableContainer metadata = default)
            : base((metadata ?? ImmutableContainer.Empty).SetWhen(x => !x.GetSchemes().Any(), x => x.SetScheme(UriSchemes.Custom.IOnymous)))
        {
            _uriConverter = uriConverter ?? throw new ArgumentNullException(nameof(uriConverter));

            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync);
        }

        public InMemoryProvider(IImmutableContainer metadata = default) : this(new UriStringPathToStringConverter(), metadata) { }

        /// <summary>
        /// Gets or sets value converter.
        /// </summary>
        public ITypeConverter Converter { get; set; } = new NullConverter();

        private async Task<IResource> GetAsync(Request request)
        {
            var name = _uriConverter.Convert<string>(request.Uri);
            return
                _items.TryGetValue(name, out var o)
                    ? o is string s
                        ? new InMemoryResource(ImmutableContainer.Empty.SetUri(request.Uri).SetFormat(MimeType.Text), await ResourceHelper.SerializeTextAsync(s))
                        : new InMemoryResource(ImmutableContainer.Empty.SetUri(request.Uri).SetFormat(MimeType.Binary), await ResourceHelper.SerializeBinaryAsync(o))
                    : new InMemoryResource(ImmutableContainer.Empty.SetUri(request.Uri), Stream.Null);
        }

        private async Task<IResource> PutAsync(Request request)
        {
            //ValidateFormatNotNull(this, uri, metadata);

            var name = _uriConverter.Convert<string>(request.Uri);
            using (var body = await request.CreateBodyStreamAsync())
            {
                _items[name] = await ResourceHelper.Deserialize<object>(body, request.Extensions);
            }

            return await InvokeAsync(new Request.Get(request.Uri));
        }

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

//        public static InMemoryProvider Add(this InMemoryProvider inMemory, string uri, object value)
//        {
//            switch (value)
//            {
//                case string str:
//                {
//                    using (var stream = ResourceHelper.SerializeTextAsync(str).GetAwaiter().GetResult())
//                    using (inMemory.PutAsync(uri, stream, Metadata.Empty.Resource().Format(MimeType.Text)).GetAwaiter().GetResult()) { }
//                }
//                    break;
//                default:
//                {
//                    using (var stream = ResourceHelper.SerializeBinaryAsync(value).GetAwaiter().GetResult())
//                    using (inMemory.PutAsync(uri, stream, Metadata.Empty.Resource().Format(MimeType.Binary)).GetAwaiter().GetResult()) { }
//                }
//                    break;
//            }
//
//            return inMemory;
//        }

        #endregion
    }

    public class InMemoryResource : Resource
    {
        [CanBeNull]
        private readonly Stream _data;

//        public InMemoryResource(UriString uri, MimeType format, Stream data)
//            : base(ImmutableSession
//                .Empty
//                .SetItem(PropertySelector.Select(x => x.Uri), uri)
//                .SetItem(From<IResourceMeta>.Select(x => x.Format), format))
//        {
//            _data = data ?? throw new ArgumentNullException(nameof(data));
//        }

        public InMemoryResource(IImmutableContainer properties, Stream data)
            : base(properties.CopyRequestProperties().Union(properties.CopyResourceProperties()).SetExists(data != Stream.Null))
        {
            _data = data;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            await _data.Rewind().CopyToAsync(stream);
        }

        public class Empty : InMemoryResource
        {
            private Empty(IImmutableContainer properties) : base(properties, Stream.Null) { }

            public static IResource From(Request request)
            {
                return new Empty(request.Extensions.CopyRequestProperties());
            }
        }
    }
}