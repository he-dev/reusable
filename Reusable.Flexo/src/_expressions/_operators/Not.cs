using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("!")]
    public class Not : PredicateExpression
    {
        public Not() : base(nameof(Not)) { }

        public IExpression Predicate { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            return (!Predicate.Invoke(context).Value<bool>(), context);
        }
    }
}