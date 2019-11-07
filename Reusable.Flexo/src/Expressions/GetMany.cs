using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class GetMany : GetItem<IEnumerable<IConstant>>
    {
        public GetMany() : base(default, nameof(GetMany)) { }

        protected override Constant<IEnumerable<IConstant>> ComputeConstantGeneric(IImmutableContainer context)
        {
            var items =
                ((IEnumerable<object>)FindItem(context))
                .Select((x, i) => Constant.FromValue($"{Path}[{i}]", x))
                .Cast<IConstant>()
                .ToList()
                .AsEnumerable();
            
            return Constant.FromValue(Path, items, context);
        }
    }
}