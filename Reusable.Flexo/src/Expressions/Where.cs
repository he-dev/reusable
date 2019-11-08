using System.Collections.Generic;
using System.Linq;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IConstant>>
    {
        public Where() : base(default) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression Predicate { get; set; }

        protected override IEnumerable<IConstant> ComputeValue(IImmutableContainer context)
        {
            var query =
                from item in This(context).Enabled()
                where Predicate.Invoke(context.BeginScopeWithThisOuter(item)).Value<bool>()
                select item;

            return query.Cast<IConstant>().ToList();
        }
    }
}