using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public Overlaps(ILogger<Overlaps> logger) : base(logger, nameof(Overlaps)) { }

        [This]
        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public List<IExpression> With { get; set; } = new List<IExpression>();

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            var with = With.Invoke(context).Values<object>();
            var comparer = context.GetComparerOrDefault(Comparer);

            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                return (Name, input.Intersect(with, comparer).Any(), context);
            }
            else
            {
                var values = Values.Invoke(context).Values<object>();
                return (Name, values.Intersect(with, comparer).Any(), context);
            }
        }
    }
}