using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(FileRequest))]
    public class InMemoryFileController : ResourceController, IEnumerable<KeyValuePair<UriString, object>>
    {
        private readonly IDictionary<UriString, object> _items = new Dictionary<UriString, object>();

        public InMemoryFileController(string? id = default) : base(id, UriSchemes.Known.File) { }

        [ResourceGet]
        public Task<Response> GetAsync(Request request)
        {
            return
                _items.TryGetValue(request.Uri, out var obj)
                    ? OK<FileResponse>(obj).ToTask()
                    : NotFound<FileResponse>().ToTask();
        }

        [ResourcePut]
        public Task<Response> AddAsync(Request request)
        {
            _items[request.Uri.ToString()] = request.Body;

            return OK<FileResponse>().ToTask();
        }

        [ResourceDelete]
        public Task<Response> DeleteAsync(Request request)
        {
            return
                _items.Remove(request.Uri)
                    ? OK<FileResponse>().ToTask()
                    : NotFound<FileResponse>().ToTask();
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