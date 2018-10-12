using System;

namespace Reusable.Validation
{
    [Flags]
    public enum WeelidationRuleOptions
    {
        None = 0,
        
        /// <summary>
        /// Instructs the validator to stop evaluating other rules.
        /// </summary>
        BreakOnFailure = 1 << 0,
    }
}