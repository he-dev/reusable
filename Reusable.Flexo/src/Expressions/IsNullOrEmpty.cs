using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ScalarExtension<bool>
    {
        public IsNullOrEmpty() : base(default) { }

        public IExpression? Left
        {
            get => Arg;
            set => Arg = value;
        }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return GetArg(context).Invoke(context).Value switch { string s => string.IsNullOrEmpty(s), _ => true };
        }
    }
}