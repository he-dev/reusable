using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;

namespace Reusable.Flowingo.Steps
{
    public interface IWorkItemContext<T>
    {
        Queue<WorkItem<T>> WorkItems { get; }

        List<(WorkItem<T> Item, bool Success)> ProcessedWorkItems { get; }
    }

    public class ProcessWorkItemQueue<T> : Step<T> where T : IWorkItemContext<T>
    {
        public override async Task ExecuteAsync(T context)
        {
            foreach (var workItem in context.WorkItems.Consume())
            {
                context.ProcessedWorkItems.Add((workItem, workItem.Execute(context)));
            }

            await ExecuteNextAsync(context);
        }
    }

    public abstract class WorkItem<T>
    {
        public object? Tag { get; set; }

        public abstract bool Execute(T context);
    }
}