using System.Collections.Generic;
using System.Linq;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any)) { }

        public IEnumerable<IExpression> Expressions { get; set; }

        protected override bool Calculate(IExpressionContext context)
        {
            return 
                Expressions
                    .InvokeWithValidation(context)
                    .Values<bool>()
                    .Any(x => x);

        }
    }
}