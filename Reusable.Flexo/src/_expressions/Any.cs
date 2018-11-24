using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Expressions { get; set; }

        protected override bool Calculate(IExpressionContext context)
        {
            return
                Expressions
                    .Enabled()
                    .InvokeWithValidation(context)
                    .Values<bool>()
                    .Any(x => x);

        }
    }
}