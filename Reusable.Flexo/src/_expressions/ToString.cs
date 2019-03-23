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
            if (context.TryPopExtensionInput(out object input))
            {
                return (Name, input.ToString(), context);
            }
            else
            {
                return (Name, Value.Value<object>().ToString(), context);
            }
        }
    }
}