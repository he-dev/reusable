namespace Reusable.Flexo
{
    public class Not : PredicateExpression
    {
        public Not() : base(nameof(Not)) { }

        public IExpression Expression { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            return InvokeResult.From(!Expression.InvokeWithValidation(context).Value<bool>(), context);
        }
    }
}