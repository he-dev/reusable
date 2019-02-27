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

        protected override bool Calculate(IExpressionContext context)
        {
            return
                Predicates
                    .Enabled()
                    .InvokeWithValidation(context)
                    .Values<bool>()
                    .Any(x => x);

        }
    }
}