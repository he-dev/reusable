using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias(">")]
    public class GreaterThan : ComparerExpression
    {
        public GreaterThan()
            : base(nameof(GreaterThan), ExpressionContext.Empty, x => x > 0) { }
    }

    [Alias(">=")]
    public class GreaterThanOrEqual : ComparerExpression
    {
        public GreaterThanOrEqual()
            : base(nameof(GreaterThanOrEqual), ExpressionContext.Empty, x => x >= 0) { }
    }
}