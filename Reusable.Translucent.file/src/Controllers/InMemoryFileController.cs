using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent.Controllers
{
    public class InMemoryFileController : ResourceController, IEnumerable<(SoftString Name, object Value)>
    {
        private readonly IDictionary<SoftString, object> _items = new Dictionary<SoftString, object>();

        public InMemoryFileController(IImmutableContainer properties = default)
            : base(
                properties
                    .ThisOrEmpty()
                    .UpdateItem(Schemes, s => s.Any() ? s : s.Add(UriSchemes.Custom.IOnymous))
                ) { }

        [ResourceGet]
        public Task<Response> GetAsync(Request request)
        {
            if (_items.TryGetValue(request.Uri.ToString(), out var obj))
            {
                // switch (obj)
                // {
                //     case string str:
                //         return new PlainResource(str, request.Metadata.Copy<ResourceProperties>()).ToTask<IResource>();
                //
                //     default:
                //         return new ObjectResource(obj, request.Metadata.Copy<ResourceProperties>()).ToTask<IResource>();
                // }
            }
            else
            {
                //return DoesNotExist(request).ToTask<IResource>();
            }

            return default;
        }

        [ResourcePut]
        public async Task<Response> AddAsync(Request request)
        {
            _items[request.Uri.ToString()] = request.Body;

            return await GetAsync(new Request.Get(request.Uri));
        }

        // protected override async Task<IResourceInfo> DeleteAsyncInternal(UriString uri, ResourceMetadata metadata)
        // {
        //     var resourceToDelete = await GetAsync(uri, metadata);
        //     _items.Remove(resourceToDelete);
        //     return await GetAsync(uri, metadata);
        // }

        #region Collection initilizers

        public InMemoryFileController Add((string Name, object Value) item) => Add(item.Name, item.Value);

        public InMemoryFileController Add(string name, object value)
        {
            _items[name] = value;
            return this;
        }

        #endregion

        public IEnumerator<(SoftString Name, object Value)> GetEnumerator() => _items.Select(x => (x.Key, x.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}