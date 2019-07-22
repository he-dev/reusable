using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class InMemoryProvider : ResourceProvider, IEnumerable<(SoftString Name, object Value)>
    {
        private readonly ITypeConverter<UriString, string> _uriConverter;
        private readonly IDictionary<SoftString, object> _items = new Dictionary<SoftString, object>();

        public InMemoryProvider([NotNull] ITypeConverter<UriString, string> uriConverter, IImmutableContainer properties = default)
            : base(properties.ThisOrEmpty().SetWhen(x => !x.GetSchemes().Any(), x => x.SetScheme(UriSchemes.Custom.IOnymous)))
        {
            _uriConverter = uriConverter ?? throw new ArgumentNullException(nameof(uriConverter));

            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync);
        }

        public InMemoryProvider(IImmutableContainer properties = default)
            : this(new UriStringPathToStringConverter(), properties) { }

        private Task<IResource> GetAsync(Request request)
        {
            var name = _uriConverter.Convert<string>(request.Uri);

            if (_items.TryGetValue(name, out var obj))
            {
                switch (obj)
                {
                    case string str:
                        return new PlainResource(str, request.Context.CopyResourceProperties()).ToTask<IResource>();

                    default:
                        return new ObjectResource(obj, request.Context.CopyResourceProperties()).ToTask<IResource>();
                }
            }
            else
            {
                return DoesNotExist(request).ToTask<IResource>();
            }
        }

        private async Task<IResource> PutAsync(Request request)
        {
            var name = _uriConverter.Convert<string>(request.Uri);
            _items[name] = request.Body;

            return await InvokeAsync(new Request.Get(request.Uri));
        }

        // protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata)
        // {
        //     var resourceToDelete = await GetAsync(uri, metadata);
        //     _items.Remove(resourceToDelete);
        //     return await GetAsync(uri, metadata);
        // }

        #region Collection initilizers

        public InMemoryProvider Add((string Name, object Value) item) => Add(item.Name, item.Value);

        public InMemoryProvider Add(string name, object value)
        {
            _items[name] = value;
            return this;
        }

        #endregion

        public IEnumerator<(SoftString Name, object Value)> GetEnumerator() => _items.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}