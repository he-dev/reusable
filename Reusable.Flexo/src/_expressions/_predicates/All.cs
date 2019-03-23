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
        public All(string name) : base(name ?? nameof(All)) { }

        internal All() : this(nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; } = Constant.True;

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values).Enabled();
            var predicate = Predicate.Value<bool>();

            var last = default(IConstant);
            foreach (var item in values)
            {
                last = item.Invoke(last?.Context ?? context);
                if (EqualityComparer<bool>.Default.Equals((bool)last.Value, predicate))
                {
                    continue;
                }
                else
                {
                    return (Name, false, context);
                }
            }

            return (Name, true, last?.Context ?? context);
        }
    }
}