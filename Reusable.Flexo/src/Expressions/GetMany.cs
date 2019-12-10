using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class GetMany : GetItem<object>
    {
        public GetMany() : base(default) { }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            var enumerable = (IEnumerable<object>)FindItem(context);
            var items =
                from x in enumerable
                from y in x is IConstant c ? c.AsEnumerable() : new[] { x }
                select y;

            return new Constant<object>(Path, items, context);
        }
    }
}