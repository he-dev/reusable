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

        protected override bool ComputeSingle(IImmutableContainer context)
        {
           return GetArg(context).AsEnumerable<string>().All(string.IsNullOrEmpty);
        }
    }
}