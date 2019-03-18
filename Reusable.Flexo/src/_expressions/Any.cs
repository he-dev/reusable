using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any), ExpressionContext.Empty) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            foreach (var predicate in Values.Enabled())
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