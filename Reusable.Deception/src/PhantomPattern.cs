using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Deception;

public interface IPhantomPattern
{
    bool CanThrow(PhantomContext context);
}

public class PhantomPattern : IPhantomPattern
{
    public PhantomPattern(IEnumerable<string> tags) => Tags = tags.ToHashSet(SoftString.Comparer);

    private ISet<string> Tags { get; }

    public virtual bool CanThrow(PhantomContext context)
    {
        return (context.CanThrow?.Invoke() ?? true) && Tags.Overlaps(context.Tags);
    }

    public override string ToString() => GetType().ToPrettyString();
}