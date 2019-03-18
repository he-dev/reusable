using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression
    {
        [JsonConstructor]
        public All(string name) : base(name ?? nameof(All), ExpressionContext.Empty) { }

        public All() : this(nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = context.Input().ValueOrDefault<List<IExpression>>() ?? Values;
            var last = default(IExpression);
            foreach (var predicate in values.Enabled())
            {
                last = predicate.Invoke(last?.Context ?? context);
                if (!(last.Value() is bool b && b))
                {
                    return (false, context);
                }
            }

            return (true, last?.Context ?? context);
        }
    }
}