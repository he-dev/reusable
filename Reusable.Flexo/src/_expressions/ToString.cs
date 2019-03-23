using System.Collections.Generic;
using System.Linq;

namespace Reusable.Flexo
{
    public class ToString : Expression<string>, IExtension<object>
    {
        public ToString(string name) : base(name) { }

        public IExpression Value { get; set; }

        protected override Constant<string> InvokeCore(IExpressionContext context)
        {
            return (Name, ExtensionInputOrDefault(ref context, Value).Value<object>().ToString(), context);
        }
    }
}