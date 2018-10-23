using System;

namespace Reusable.Validation
{
    [Flags]
    public enum BouncerPolicyOptions
    {
        None = 0,
        
        /// <summary>
        /// Instructs the validator to stop evaluating other rules.
        /// </summary>
        BreakOnFailure = 1 << 0,
    }
}