using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class ToList : Expression
    {
        public ToList()
            : base(nameof(ToList))
        { }

        [JsonRequired]
        public IList<IExpression> Expressions { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            return Constant.FromValue(nameof(ToList), Expressions.InvokeWithValidation(context).ToList());
        }
    }
}