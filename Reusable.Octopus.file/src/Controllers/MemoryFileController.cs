using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

public class MemoryFileController : ResourceController<FileRequest>, IEnumerable<KeyValuePair<string, object?>>
{
    private IDictionary<string, object?> Files { get; } = new Dictionary<string, object?>();

    public override Task<Response> ReadAsync(FileRequest request)
    {
        return
            Files.TryGetValue(request.ResourceName.Peek(), out var obj)
                ? Success<FileResponse>(request.ResourceName, obj).ToTask()
                : NotFound<FileResponse>(request.ResourceName).ToTask();
    }

    public override Task<Response> CreateAsync(FileRequest request)
    {
        Files[request.ResourceName.Peek()] = request.Body;

        return Success<FileResponse>(request.ResourceName).ToTask();
    }

    public override Task<Response> DeleteAsync(FileRequest request)
    {
        return
            Files.Remove(request.ResourceName.Peek())
                ? Success<Response>(request.ResourceName).ToTask()
                : NotFound<Response>(request.ResourceName).ToTask();
    }

    #region Collection initilizers

    public MemoryFileController Add((string Name, object Value) item) => Add(item.Name, item.Value);

    public MemoryFileController Add(string name, object value)
    {
        Files[name] = value;
        return this;
    }

    #endregion

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Files.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Files).GetEnumerator();

    public override void Dispose()
    {
        foreach (var item in Files)
        {
            (item.Value as IDisposable)?.Dispose();
        }
    }
}