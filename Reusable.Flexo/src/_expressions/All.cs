using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class All : PredicateExpression
    {
        public All() : base(nameof(All)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Expressions { get; set; }

        protected override bool Calculate(IExpressionContext context)
        {
            return 
                Expressions
                    .Enabled()
                    .InvokeWithValidation(context)
                    .Values<bool>()
                    .All(x => x);
        }
    }
}