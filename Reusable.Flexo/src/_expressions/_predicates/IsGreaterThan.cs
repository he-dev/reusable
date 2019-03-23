using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias(">")]
    public class IsGreaterThan : ComparerExpression
    {
        public IsGreaterThan() : base(nameof(IsGreaterThan), x => x > 0) { }
    }
    
    [Alias(">=")]
    public class IsGreaterThanOrEqual : ComparerExpression
    {
        public IsGreaterThanOrEqual() : base(nameof(IsGreaterThanOrEqual), x => x >= 0) { }
    }
}