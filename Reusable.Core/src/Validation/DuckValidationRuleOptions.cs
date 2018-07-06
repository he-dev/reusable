using System;

namespace Reusable.Validation
{
    [Flags]
    public enum DuckValidationRuleOptions
    {
        None = 0,
        BreakOnFailure = 1 << 0,
    }
}