using System;

namespace Reusable.Validation
{
    [Flags]
    public enum ValidationOptions
    {
        None = 0,
        StopOnFailure = 1 << 0,
    }
}