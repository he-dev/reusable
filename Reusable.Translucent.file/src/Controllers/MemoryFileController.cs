using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    public class MemoryFileController<T> : ResourceController<T>, IEnumerable<KeyValuePair<string, object?>> where T : Request
    {
        private readonly IDictionary<string, object?> _items = new Dictionary<string, object?>();

        public override Task<Response> ReadAsync(T request)
        {
            return
                _items.TryGetValue(request.ResourceName, out var obj)
                    ? Success<FileResponse>(request.ResourceName, obj).ToTask()
                    : NotFound<FileResponse>(request.ResourceName).ToTask();
        }

        public override Task<Response> CreateAsync(T request)
        {
            _items[request.ResourceName] = request.Body;

            return Success<FileResponse>(request.ResourceName).ToTask();
        }

        public override Task<Response> DeleteAsync(T request)
        {
            return
                _items.Remove(request.ResourceName)
                    ? Success<Response>(request.ResourceName).ToTask()
                    : NotFound<Response>(request.ResourceName).ToTask();
        }

        #region Collection initilizers

        public MemoryFileController<T> Add((string Name, object Value) item) => Add(item.Name, item.Value);

        public MemoryFileController<T> Add(string name, object value)
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