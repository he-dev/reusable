using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo.Expressions
{
    /// <summary>
    /// This expression does nothing. It's used to avoid nulls.
    /// </summary>
    public class Empty : Expression
    {
        public Empty() : base(nameof(Empty)) { }

        public override IExpression Invoke(IExpressionContext context) => this;
    }
}