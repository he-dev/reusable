using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Reusable.Deception.Patterns;

public class Delay : PhantomPattern
{
    public Delay(IEnumerable<string> tags, TimeSpan duration) : base(tags)
    {
        Duration = duration;
        Stopwatch = Stopwatch.StartNew();
    }

    public TimeSpan Duration { get; }

    private Stopwatch Stopwatch { get; }

    public override bool CanThrow(PhantomContext context)
    {
        return base.CanThrow(context) && Stopwatch.Elapsed > Duration;
    }
}