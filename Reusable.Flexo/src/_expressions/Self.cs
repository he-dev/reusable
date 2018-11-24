namespace Reusable.Flexo
{
    /// <summary>
    /// This expression does nothing. It's used to avoid nulls.
    /// </summary>
    public class Self : Expression
    {
        public Self() : base(nameof(Self)) { }

        public override IExpression Invoke(IExpressionContext context) => this;
    }
}