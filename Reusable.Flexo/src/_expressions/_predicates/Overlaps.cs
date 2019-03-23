using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Overlaps : PredicateExpression, IExtension<List<object>>
    {
        [JsonConstructor]
        public Overlaps(string name) : base(name ?? nameof(Overlaps)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public List<IExpression> With { get; set; } = new List<IExpression>();

        public IExpression Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var with = With.Invoke(context).Values<object>();
            var comparer = Comparer?.Invoke(context).Value<IEqualityComparer<object>>() ?? EqualityComparer<object>.Default;

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