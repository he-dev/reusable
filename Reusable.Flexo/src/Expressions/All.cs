using System.Collections.Generic;
using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
{
    public class All : PredicateExpression
    {
        public All() : base(nameof(All)) { }

        public IEnumerable<IExpression> Expressions { get; set; }

        protected override bool Calculate(IExpressionContext context)
        {
            return 
                Expressions
                    .SafeInvoke(context)
                    .Values<bool>()
                    .All(x => x);
        }
    }
}