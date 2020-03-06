using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class QueueExtensions
    {
        public static void Add<T>([NotNull] this Queue<T> queue, T item)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            queue.Enqueue(item);
        }

        public static IEnumerable<T> Consume<T>(this Queue<T> queue)
        {
            while (queue.Count > 0)
            {
                yield return queue.Dequeue();
            }
        }
    }
}