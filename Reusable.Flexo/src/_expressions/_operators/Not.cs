using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("!")]
    public class Not : PredicateExpression, IExtension<bool>
    {
        public Not(string name) : base(name) { }

        public Not() : this(nameof(Not)) { }

        public IExpression Value { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var predicate = context.Input() ?? Value;
            return (!predicate.Invoke(context).Value<bool>(), context);
        }
    }
}