using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public static class Enumerator
    {
        public static IEnumerable<Node<Exception>> GetInnerExceptions(this Exception exception, bool includeCurrent = true)
        {
            if (exception == null) { throw new ArgumentNullException(nameof(exception)); }

            var exceptionStack = new Stack<Node<Exception>>();

            var depth = 0;

            if (includeCurrent)
            {
                exceptionStack.Push(new Node<Exception>(exception, depth));
            }

            while (exceptionStack.Any())
            {
                var current = exceptionStack.Pop();
                yield return current;

                if (current.Value is AggregateException)
                {
                    depth++;
                    foreach (var innerException in ((AggregateException)current).InnerExceptions)
                    {
                        exceptionStack.Push(new Node<Exception>(innerException, depth + 1));
                    }
                    continue;
                }
                if (current.Value.InnerException != null)
                {
                    depth++;
                    exceptionStack.Push(new Node<Exception>(current.Value.InnerException, depth));
                    depth--;
                }
            }
        }
    }
}