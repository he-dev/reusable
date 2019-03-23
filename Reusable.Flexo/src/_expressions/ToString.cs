using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flexo
{
    public class ToString : Expression<string>, IExtension<object>
    {
        public ToString(string name, IExpressionContext context) : base(name, context) { }

        public IExpression Value { get; set; }

        protected override ExpressionResult<string> InvokeCore(IExpressionContext context)
        {
            return (ExtensionInputOrDefault(ref context, Value).Value<object>().ToString(), context);
        }
    }
}