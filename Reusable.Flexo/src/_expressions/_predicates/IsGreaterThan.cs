using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias(">")]
    public class IsGreaterThan : ComparerExpression
    {
        public IsGreaterThan(ILogger<IsGreaterThan> logger) : base(logger, nameof(IsGreaterThan), x => x > 0) { }
    }
    
    [Alias(">=")]
    public class IsGreaterThanOrEqual : ComparerExpression
    {
        public IsGreaterThanOrEqual(ILogger<IsGreaterThanOrEqual> logger) : base(logger, nameof(IsGreaterThanOrEqual), x => x >= 0) { }
    }
}