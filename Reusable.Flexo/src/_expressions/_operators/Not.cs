using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    [Alias("!")]
    public class Not : PredicateExpression
    {
        public Not(string name) : base(name) { }

        public Not() : this(nameof(Not)) { }

        public IExpression Predicate { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var predicate = context.Get(Item.For<IExtensionContext>(), x => x.Input) ?? Predicate;
            return (!predicate.Invoke(context).Value<bool>(), context);
        }
    }
}