using Reusable.Data;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : ScalarExtension<bool>
    {
        public Not() : base(default) { }

        public IExpression Value { get => Arg; set => Arg = value; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return !GetArg(context).Invoke(context).Value<bool>();
        }
    }
}