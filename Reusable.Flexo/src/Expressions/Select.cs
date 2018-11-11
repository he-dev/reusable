using System.Collections.Generic;
using System.Linq;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
{
    public class Select : Expression
    {
        public Select()
            : base(nameof(Select))
        { }

        public IList<IExpression> Expressions { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            return Constant.Create(nameof(Select), Expressions.SafeInvoke(context).ToList());
        }
    }
}