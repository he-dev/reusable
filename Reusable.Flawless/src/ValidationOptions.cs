using System;

namespace Reusable.Flawless
{
    [Flags]
    public enum ValidationOptions
    {
        None = 0,
        StopOnFailure = 1 << 0,
    }
}