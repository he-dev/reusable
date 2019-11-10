using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Package : Expression<object>
    {
        public Package() : base(default) { }

        public IExpression Body { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return Body.Invoke(context);
        }
    }
}