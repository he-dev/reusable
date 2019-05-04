using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class GetMany : GetItem<IEnumerable<IExpression>>
    {
        public GetMany([NotNull] ILogger<GetMany> logger) : base(logger, nameof(GetMany)) { }

        protected override Constant<IEnumerable<IExpression>> InvokeCore()
        {
            return (Path, ((IEnumerable<object>)FindItem()).Select((x, i) => Constant.Create<object>($"Item[{i}]", x)).ToList());
        }
    }
}