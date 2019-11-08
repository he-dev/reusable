using Reusable.Data;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : ScalarExtension<bool>
    {
        public Not() : base(default) { }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return !This(context).Invoke(context).Value<bool>();
        }
    }
}