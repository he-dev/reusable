using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression, IExtension<IEnumerable<object>>
    {
        [JsonConstructor]
        public All(string name) : base(name ?? nameof(All)) { }

        internal All() : this(nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; } = Constant.True;

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                var predicate = Predicate.Invoke(context).Value<bool>();
                return (Name, input.Cast<bool>().All(x => x == predicate), context);
            }
            else
            {
                var predicate = Predicate.Invoke(context).Value<bool>();
                var last = default(IConstant);
                foreach (var item in Values)
                {
                    last = item.Invoke(last?.Context ?? context);
                    if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate))
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
}