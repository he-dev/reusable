using System;
using System.Collections.Generic;

namespace Reusable.Deception.Patterns;

public class AdHoc : PhantomPattern
{
    private readonly Func<PhantomContext, bool> _canThrow;

    public AdHoc(IEnumerable<string> tags, Func<PhantomContext, bool> canThrow) : base(tags) => _canThrow = canThrow;

    public override bool CanThrow(PhantomContext context)
    {
        return base.CanThrow(context) && _canThrow(context);
    }
}