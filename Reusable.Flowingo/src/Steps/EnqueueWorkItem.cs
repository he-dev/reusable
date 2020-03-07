using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;

namespace Reusable.Flowingo.Steps
{
    public class EnqueueWorkItem<T> : Step<T>, IEnumerable<WorkItem<T>> where T : IWorkItemContext<T>
    {
        private readonly Queue<WorkItem<T>> _workItemQueue = new Queue<WorkItem<T>>();

        public void Add(WorkItem<T> workItem) => _workItemQueue.Enqueue(workItem);

        public override async Task ExecuteAsync(T context)
        {
            foreach (var workItem in _workItemQueue.Consume())
            {
                context.WorkItems.Enqueue(workItem);
            }

            await ExecuteNextAsync(context);
        }

        public IEnumerator<WorkItem<T>> GetEnumerator() => _workItemQueue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_workItemQueue).GetEnumerator();
    }
}