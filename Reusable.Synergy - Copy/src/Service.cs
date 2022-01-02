using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy;

public interface IRequest
{
    IDictionary<string, object> Items { get; }
}

public interface IRequest<T> : IRequest { }

public abstract class Request<T> : IRequest<T>
{
    protected Request()
    {
        this.SetItem(OnError.Halt);
    }
    
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);
}

public interface IItems
{
    // Allows to store additional metadata.
    IDictionary<string, object> Items { get; }
}

public interface IIdentifiable
{
    string Name { get; }
}

public interface IService<T> : IItems, IEnumerable<IService<T>>
{
    // Points to the next service in a pipeline.
    IService<T>? Next { get; set; }

    Task<T> InvokeAsync();
}

public abstract class Service<T> : IService<T>
{
    private IService<T>? _last;

    public IService<T>? Next { get; set; }

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    protected IService<T> Last => _last ??= this.Last();

    public abstract Task<T> InvokeAsync();

    public IEnumerator<IService<T>> GetEnumerator() => this.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected async Task<T> InvokeNext() => await (Next?.InvokeAsync() ?? Task.FromResult<T>(default));

    // Null-Service.
    public class Empty : Service<T>
    {
        public override async Task<T> InvokeAsync() => await InvokeNext();
    }

    public class PipelineBuilder : IEnumerable<IService<T>>
    {
        private IService<T> First { get; } = new Empty();

        // Adds the specified service at the end of the pipeline.
        public PipelineBuilder Add(IService<T> last) => this.Also(b => b.First.Enumerate().Last().Next = last);

        public IEnumerator<IService<T>> GetEnumerator() => First.Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IService<T> Build() => First;
    }
}

public sealed class Unit { }

public interface INode : IEnumerable<INode>
{
    // Points to the next middleware.
    INode? Next { get; set; }

    Task<object> InvokeAsync(IRequest request);
}

public abstract class Node : INode
{
    public INode Next { get; set; } = new Unit();

    public abstract Task<object> InvokeAsync(IRequest request);

    public IEnumerator<INode> GetEnumerator() => this.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected async Task<object> InvokeNext(IRequest request) => await Next.InvokeAsync(request);

    public class Empty : Node
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeNext(request);
    }

    public class Unit : Node
    {
        public override Task<object> InvokeAsync(IRequest request) => new Unit().ToTask<object>();
    }

    public class PipelineBuilder : IEnumerable<INode>
    {
        private INode First { get; } = new Empty();

        // Adds the specified service at the end of the pipeline.
        public PipelineBuilder Add(INode last) => this.Also(b => b.First.Enumerate().Last().Next = last);

        public IEnumerator<INode> GetEnumerator() => First.Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public INode Build() => First;
    }
}