using System;

namespace Reusable.Flawless
{
    [Flags]
    public enum ExpressValidationOptions
    {
        None = 0,
        
        /// <summary>
        /// Instructs the validator to stop evaluating other rules.
        /// </summary>
        BreakOnFailure = 1 << 0,
    }
}