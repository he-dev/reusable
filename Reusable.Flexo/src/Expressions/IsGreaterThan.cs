using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    //[Alias(">")]
    public class IsGreaterThan : Comparer
    {
        public IsGreaterThan() : base(default, nameof(IsGreaterThan), x => x > 0) { }
    }
    
    //[Alias(">=")]
    public class IsGreaterThanOrEqual : Comparer
    {
        public IsGreaterThanOrEqual() : base(default, nameof(IsGreaterThanOrEqual), x => x >= 0) { }
    }
}