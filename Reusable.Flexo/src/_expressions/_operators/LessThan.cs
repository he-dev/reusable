using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("<")]
    public class LessThan : ComparerExpression
    {
        public LessThan()
            : base(nameof(LessThan), x => x < 0)
        { }
    }

    [Alias("<=")]
    public class LessThanOrEqual : ComparerExpression
    {
        public LessThanOrEqual()
            : base(nameof(LessThanOrEqual), x => x <= 0)
        { }
    }
}