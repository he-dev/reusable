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

        public object Predicate { get; set; } = true;

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);
            var predicate = Constant.FromValueOrDefault(nameof(Predicate), Predicate).Value<bool>();

            var last = default(IExpression);
            foreach (var item in values.Enabled())
            {
                last = item.Invoke(last?.Context ?? context);
                if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate))
                {
                    continue;
                }
                else
                {
                    return (false, context);
                }
            }

            return (true, last?.Context ?? context);
        }
    }
}