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

        public List<object> Values { get; set; } = new List<object>();

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);            
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