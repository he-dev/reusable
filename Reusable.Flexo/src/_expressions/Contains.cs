using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : PredicateExpression, IExtension<List<object>>
    {
        public Contains() : base(nameof(Contains), ExpressionContext.Empty) { }

        public List<object> Values { get; set; } = new List<object>();

        public object Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var value = Constant.FromValueOrDefault("Value")(Value).Invoke(context).Value<object>();
            var comparer = Comparer?.Invoke(context).ValueOrDefault<IEqualityComparer<object>>() ?? EqualityComparer<object>.Default;
            var values = ExtensionInputOrDefault(ref context, Values).Values<object>();
            return (values.Any(x => comparer.Equals(value, x)), context);
        }
    }        
}