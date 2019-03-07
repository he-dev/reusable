namespace Reusable.Flexo
{
    public class Not : PredicateExpression
    {
        public Not() : base(nameof(Not)) { }

        public IExpression Predicate { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            return (!Predicate.InvokeWithValidation(context).Value<bool>(), context);
        }
    }
}