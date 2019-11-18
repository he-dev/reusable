using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    public class InMemoryFileController : ResourceController, IEnumerable<KeyValuePair<UriString, object>>
    {
        private readonly IDictionary<UriString, object> _items = new Dictionary<UriString, object>();

        public InMemoryFileController(IImmutableContainer? properties = default) : base(new SoftString[] { UriSchemes.Known.File }, default, properties) { }

        [ResourceGet]
        public Task<Response> GetAsync(Request request)
        {
            return
                _items.TryGetValue(request.Uri, out var obj)
                    ? OK(obj).ToTask()
                    : NotFound().ToTask();
        }

        [ResourcePut]
        public Task<Response> AddAsync(Request request)
        {
            _items[request.Uri.ToString()] = request.Body;

            return OK().ToTask();
        }

        [ResourceDelete]
        public Task<Response> DeleteAsync(Request request)
        {
            return
                _items.Remove(request.Uri)
                    ? OK().ToTask()
                    : NotFound().ToTask();
        }

        #region Collection initilizers

        public InMemoryFileController Add((string Name, object Value) item) => Add(item.Name, item.Value);

        public InMemoryFileController Add(string name, object value)
        {
            _items[name] = value;
            return this;
        }

        #endregion

        public IEnumerator<KeyValuePair<UriString, object>> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        public override void Dispose()
        {
            foreach (var item in _items)
            {
                (item.Value as IDisposable)?.Dispose();
            }
        }
    }
}