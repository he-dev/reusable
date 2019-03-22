using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression, IExtension<IEnumerable<IExpression>>
    {
        [JsonConstructor]
        public All(string name) : base(name ?? nameof(All), ExpressionContext.Empty) { }

        internal All() : this(nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; } = Constant.True;

        protected override ExpressionResult<bool> InvokeCore(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values).Enabled();
            var predicate = Predicate.Value<bool>();

            var last = default(IExpression);
            foreach (var item in values)
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