using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : Extension<string, bool>
    {
        public IsNullOrEmpty() : base(default) { }

        public IExpression? Left
        {
            set => Arg = value;
        }

        protected override bool ComputeValue(IImmutableContainer context)
        {
           return GetArg(context).AsEnumerable<string>().All(string.IsNullOrEmpty);
        }
    }
}