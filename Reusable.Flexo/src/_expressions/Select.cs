using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Select : Expression
    {
        public Select()
            : base(nameof(Select))
        { }

        [JsonRequired]
        public IList<IExpression> Expressions { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            return Constant.Create(nameof(Select), Expressions.InvokeWithValidation(context).ToList());
        }
    }
}