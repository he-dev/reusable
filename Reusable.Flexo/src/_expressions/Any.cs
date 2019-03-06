using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Predicates { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            foreach (var predicate in Predicates.Enabled())
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