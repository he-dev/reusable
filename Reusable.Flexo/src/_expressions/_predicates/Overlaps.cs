using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Overlaps : PredicateExpression, IExtension<List<IExpression>>
    {
        public Overlaps(string name, IExpressionContext context) : base(name, context)
        { }

        [JsonConstructor]
        public Overlaps(string name) : base(name ?? nameof(Overlaps), ExpressionContext.Empty)
        { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public List<IExpression> With { get; set; } = new List<IExpression>();

        public IExpression Comparer { get; set; }

        protected override ExpressionResult<bool> InvokeCore(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values).Values<object>();
            var with = With.Values<object>();

            var comparer = Comparer?.Invoke(context).ValueOrDefault<IEqualityComparer<object>>() ?? EqualityComparer<object>.Default;

            return (values.Intersect(with, comparer).Any(), context);
        }
    }    
}