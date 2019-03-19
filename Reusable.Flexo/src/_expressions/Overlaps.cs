using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Overlaps : PredicateExpression
    {
        public Overlaps(string name, IExpressionContext context) : base(name, context)
        { }

        [JsonConstructor]
        public Overlaps(string name) : base(name ?? nameof(Overlaps), ExpressionContext.Empty)
        { }

        public List<object> Values { get; set; } = new List<object>();

        public List<object> With { get; set; } = new List<object>();

        public IExpression Comparer { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values).Values<object>();
            var with = With.Select(Constant.FromValueOrDefault("Left")).Values<object>();

            var comparer = Comparer?.Invoke(context).ValueOrDefault<IEqualityComparer<object>>() ?? EqualityComparer<object>.Default;

            return (values.Intersect(with, comparer).Any(), context);
        }
    }
}