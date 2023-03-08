using System.Collections.Generic;

namespace Reusable.Extensions;

public static class QueueExtensions
{
    public static void Add<T>(this Queue<T> queue, T item)
    {
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