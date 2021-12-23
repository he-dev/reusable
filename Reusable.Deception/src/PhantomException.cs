using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;

namespace Reusable.Deception;

public interface IPhantomException
{
    void Throw(PhantomContext context);
}

public class PhantomException : IPhantomException, IEnumerable<IPhantomPattern>
{
    private List<IPhantomPattern> Patterns { get; } = new();

    public void Throw(PhantomContext context)
    {
        lock (Patterns)
        {
            if (Patterns.FirstOrDefault(t => t.CanThrow(context)) is { } pattern)
            {
                throw context.CreateException?.Invoke() ?? DynamicException.Create("PhantomException", $"Thrown because it matches the pattern {pattern}.");
            }
        }
    }

    public void Add(IPhantomPattern pattern) => Patterns.Add(pattern);

    public IEnumerator<IPhantomPattern> GetEnumerator() => Patterns.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Patterns).GetEnumerator();

    // This implementation does nothing and supports the null-object-pattern.
    public class Empty : IPhantomException
    {
        public void Throw(PhantomContext context)
        {
            // Does nothing.
        }
    }
}