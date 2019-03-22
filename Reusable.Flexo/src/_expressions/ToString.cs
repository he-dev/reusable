using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flexo
{
    public class ToString : Expression<List<string>>
    {
        public ToString(string name, IExpressionContext context) : base(name, context)
        { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        protected override ExpressionResult<List<string>> InvokeCore(IExpressionContext context)
        {
            var strings =
                from value in ExtensionInputOrDefault(ref context, Values)
                select value.Value<object>().ToString();

            return (strings.ToList(), context);
        }
    }       
}