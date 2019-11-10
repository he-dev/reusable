using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class GetMany : GetItem<IEnumerable<IExpression>>
    {
        public GetMany() : base(default) { }

        protected override Constant<IEnumerable<IExpression>> ComputeConstantGeneric(IImmutableContainer context)
        {
            var items =
                ((IEnumerable<object>)FindItem(context))
                .Select((x, i) => x is IConstant constant ? constant : Constant.FromValue($"{Path}[{i}]", x, context))
                .ToList()
                .Cast<IExpression>()
                .AsEnumerable();
            
            return Constant.FromValue(Path, items, context);
        }
    }
}