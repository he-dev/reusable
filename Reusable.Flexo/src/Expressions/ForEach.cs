using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class ForEach : CollectionExtension<object>
    {
        public ForEach() : base(default, nameof(ForEach)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override object InvokeAsValue(IImmutableContainer context)
        {
            var query =
                from item in This(context).Enabled()
                from expr in Body.Enabled()
                select (item, expr);

            foreach (var (item, expr) in query)
            {
                expr.Invoke(context, ImmutableContainer.Empty.SetItem(ExpressionContext.Item, item));
            }

            return default;
        }
    }
}