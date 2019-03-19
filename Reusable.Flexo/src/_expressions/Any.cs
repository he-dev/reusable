using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any), ExpressionContext.Empty) { }

        public List<object> Values { get; set; } = new List<object>();

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);

            foreach (var predicate in values.Enabled())
            {
                var result = predicate.Invoke(context);
                if (result.Value<bool>())
                {
                    return (true, result.Context);
                }
            }

            return (false, context);
        }
    }
}