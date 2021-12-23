using System.Collections.Generic;

namespace Reusable.Deception.Patterns;

public class NumberOfTimes : PhantomPattern
{
    public NumberOfTimes(IEnumerable<string> tags, int count) : base(tags) => Count = count;

    public int Count { get; }

    private int Counter { get; set; }

    public override bool CanThrow(PhantomContext context)
    {
        return base.CanThrow(context) && ++Counter < Count;
    }
}