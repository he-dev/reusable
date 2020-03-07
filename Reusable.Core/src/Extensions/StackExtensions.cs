using System.Collections.Generic;

namespace Reusable.Extensions
{
    public static class StackExtensions
    {
        public static void Add<T>(this Stack<T> stack, T item) => stack.Push(item);

        public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
        {
            foreach (var item in items) stack.Push(item);
        }
        
        public static IEnumerable<T> Consume<T>(this Stack<T> queue)
        {
            while (queue.Count > 0)
            {
                yield return queue.Pop();
            }
        }
    }
}