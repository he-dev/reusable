using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("<")]
    public class LessThan : ComparerExpression
    {
        public LessThan()
            : base(nameof(LessThan), ExpressionContext.Empty, x => x < 0)
        { }
    }

    [Alias("<=")]
    public class LessThanOrEqual : ComparerExpression
    {
        public LessThanOrEqual()
            : base(nameof(LessThanOrEqual), ExpressionContext.Empty, x => x <= 0)
        { }
    }
}