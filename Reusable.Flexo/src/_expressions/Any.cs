using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any), ExpressionContext.Empty) { }

        public List<object> Values { get; set; } = new List<object>();
        
        public object Predicate { get; set; } = true;

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);
            var predicate = Constant.FromValueOrDefault(nameof(Predicate), Predicate).Value<bool>();

            foreach (var item in values.Enabled())
            {
                var result = item.Invoke(context);
                if (EqualityComparer<bool>.Default.Equals(result.Value<bool>(), predicate))
                {
                    return (true, result.Context);
                }
            }

            return (false, context);
        }
    }
}