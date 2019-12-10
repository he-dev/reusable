using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : Extension<bool, bool>
    {
        public Not() : base(default) { }

        public IExpression? Value
        {
            set => Arg = value;
        }

        protected override IEnumerable<bool> ComputeValues(IImmutableContainer context)
        {
            return GetArg(context).Invoke(context).AsEnumerable<bool>().Select(x => !x);
        }
    }
}