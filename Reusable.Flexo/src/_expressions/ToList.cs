using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class ToList : Expression
    {
        public ToList()
            : base(nameof(ToList))
        { }

        [JsonRequired]
        public List<IExpression> Expressions { get; set; } = new List<IExpression>();

        public override IExpression Invoke(IExpressionContext context)
        {
            return Constant.FromValue(nameof(ToList), Expressions.Enabled().Select(e => e.Invoke(context)).ToList());
        }
    }
}