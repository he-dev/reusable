using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("<")]
    public class IsLessThan : ComparerExpression
    {
        public IsLessThan() : base(nameof(IsLessThan), x => x < 0) { }
    }

    [Alias("<=")]
    public class IsLessThanOrEqual : ComparerExpression
    {
        public IsLessThanOrEqual() : base(nameof(IsLessThanOrEqual), x => x <= 0) { }
    }
}