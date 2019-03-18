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
        public All() : base(nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var last = default(IExpression);
            foreach (var predicate in Values.Enabled())
            {
                last = predicate.Invoke(last?.Context ?? context);
                if (!last.Value<bool>())
                {
                    return (false, context);
                }
            }

            return (true, last?.Context ?? context);
        }
    }
}