using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;

namespace Reusable.Flowingo.Steps
{
    public interface IWorkItemContext<T>
    {
        Queue<WorkItem<T>> WorkItems { get; }

        List<(WorkItem<T> Item, bool Success)> ProcessedWorkItems { get; }
    }

    public class ProcessWorkItemQueue<T> : Step<T> where T : IWorkItemContext<T>
    {
        protected override async Task<Flow> ExecuteBody(T context)
        {
            foreach (var workItem in context.WorkItems.Consume())
            {
                context.ProcessedWorkItems.Add((workItem, await workItem.ExecuteAsync(context)));
            }

            return Flow.Continue;
        }
    }

    public abstract class WorkItem<T>
    {
        public object? Tag { get; set; }

        public abstract Task<bool> ExecuteAsync(T context);
    }
}