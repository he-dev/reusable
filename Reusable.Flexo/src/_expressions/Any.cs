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
            var last = default(IExpression);
            foreach (var predicate in Predicates.Enabled())
            {
                last = predicate.Invoke(last?.Context ?? context);
                if (last.Value<bool>())
                {
                    return InvokeResult.From(true, last.Context);
                }
            }

            return InvokeResult.From(false, context);
        }
    }
}