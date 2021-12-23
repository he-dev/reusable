using System.Threading.Tasks;
using Reusable.Essentials;

namespace Reusable.Translucent.Abstractions;

public interface IResourceNode : IListNode<IResourceNode>
{
    Task InvokeAsync(ResourceContext context);
}

public abstract class ResourceNode : IResourceNode
{
    public IResourceNode? Prev { get; set; }

    public IResourceNode? Next { get; set; }

    public abstract Task InvokeAsync(ResourceContext context);

    protected Task InvokeNext(ResourceContext context) => Next is {} node ? node.InvokeAsync(context) : Task.CompletedTask;
}