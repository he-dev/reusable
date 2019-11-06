using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IExpression>>
    {
        public Where() : base(default, nameof(Where)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression Predicate { get; set; }

        protected override IEnumerable<IExpression> InvokeAsValue(IImmutableContainer context)
        {
            var query =
                from item in This(context).Enabled()
                where Predicate.Invoke(context.BeginScopeWithThisOuter(item)).Value<bool>()
                select item;

            return query.ToList();
        }
    }
}