using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Overlaps : PredicateExpression, IExtension<List<IExpression>>
    {
        [JsonConstructor]
        public Overlaps(string name) : base(name ?? nameof(Overlaps)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public List<IExpression> With { get; set; } = new List<IExpression>();

        public IExpression Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values).Values<object>();
            var with = With.Values<object>();

            var comparer = (IEqualityComparer<object>)Comparer?.Invoke(context).Value ?? EqualityComparer<object>.Default;

            return (Name, values.Intersect(with, comparer).Any(), context);
        }
    }
}