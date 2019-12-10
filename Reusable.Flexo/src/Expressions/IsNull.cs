using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : Extension<object, bool>
    {
        public IsNull() : base(default) { }

        public IExpression? Left
        {
            set => Arg = value;
        }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return GetArg(context).All(x => x is null);
        }
    }
}