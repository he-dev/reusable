namespace Reusable.Flexo.Expressions
{
    public class GreaterThan : ComparerExpression
    {
        public GreaterThan()
            : base(nameof(GreaterThan), x => x > 0)
        { }
    }

    public class GreaterThanOrEqual : ComparerExpression
    {
        public GreaterThanOrEqual()
            : base(nameof(GreaterThanOrEqual), x => x >= 0)
        { }
    }
}