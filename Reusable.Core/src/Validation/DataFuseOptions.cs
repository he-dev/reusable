using System;

namespace Reusable.Validation
{
    [Flags]
    public enum DataFuseOptions
    {
        None = 0,
        StopOnFailure = 1 << 0,
    }
}