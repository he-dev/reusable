using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;
using Reusable.Flowingo.Data;

namespace Reusable.Flowingo.Steps
{
    public class EnqueueWorkItem<T> : Step<T>, IEnumerable<WorkItem<T>> where T : IWorkItemContext<T>
    {
        private readonly Queue<WorkItem<T>> _workItemQueue = new Queue<WorkItem<T>>();

        public void Add(WorkItem<T> workItem) => _workItemQueue.Enqueue(workItem);

        protected override Task<Flow> ExecuteBody(T context)
        {
            foreach (var workItem in _workItemQueue.Consume())
            {
                context.WorkItems.Enqueue(workItem);
            }

            return Flow.Continue.ToTask();
        }

        public IEnumerator<WorkItem<T>> GetEnumerator() => _workItemQueue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_workItemQueue).GetEnumerator();
    }
}