using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Controllers;

public class MemoryFileController : ResourceController<FileRequest>, IEnumerable<KeyValuePair<string, object?>>
{
    public MemoryFileController() : base(new[] { "file" }) { }
    
    private IDictionary<string, object?> Files { get; } = new Dictionary<string, object?>();

    protected override Task<Response> ReadAsync(FileRequest request)
    {
        return
            Files.TryGetValue(request.ResourceName.Peek(), out var obj)
                ? Success<FileResponse>(request.ResourceName.Value, obj).ToTask()
                : NotFound<FileResponse>(request.ResourceName.Value).ToTask();
    }

    protected override Task<Response> CreateAsync(FileRequest request)
    {
        Files[request.ResourceName.Peek()] = request.Data;

        return Success<FileResponse>(request.ResourceName.Value).ToTask();
    }

    protected override Task<Response> DeleteAsync(FileRequest request)
    {
        return
            Files.Remove(request.ResourceName.Peek())
                ? Success<Response>(request.ResourceName.Value).ToTask()
                : NotFound<Response>(request.ResourceName.Value).ToTask();
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