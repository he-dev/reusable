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
    }
}