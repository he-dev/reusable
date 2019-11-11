using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : ScalarExtension<bool>
    {
        public IsNull() : base(default) { }
        
        public IExpression? Left { get => Arg; set => Arg = value; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return GetArg(context).Invoke(context).Value is null;
        }
    }
}