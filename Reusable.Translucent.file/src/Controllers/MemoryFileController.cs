using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(FileRequest))]
    public class MemoryFileController : Controller, IEnumerable<KeyValuePair<string, object?>>
    {
        private readonly IDictionary<string, object?> _items = new Dictionary<string, object?>();

        public MemoryFileController(ControllerName? name = default) : base(name) { }

        [ResourceGet]
        public Task<Response> GetAsync(Request request)
        {
            return
                _items.TryGetValue(request.ResourceName, out var obj)
                    ? OK<FileResponse>(obj).ToTask<Response>()
                    : NotFound<FileResponse>().ToTask<Response>();
        }

        [ResourcePut]
        public Task<Response> AddAsync(Request request)
        {
            _items[request.ResourceName.ToString()] = request.Body;

            return OK<FileResponse>().ToTask<Response>();
        }

        [ResourceDelete]
        public Task<Response> DeleteAsync(Request request)
        {
            return
                _items.Remove(request.ResourceName)
                    ? OK<FileResponse>().ToTask<Response>()
                    : NotFound<FileResponse>().ToTask<Response>();
        }

        #region Collection initilizers

        public MemoryFileController Add((string Name, object Value) item) => Add(item.Name, item.Value);

        public MemoryFileController Add(string name, object value)
        {
            _items[name] = value;
            return this;
        }

        #endregion

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _items.GetEnumerator();

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