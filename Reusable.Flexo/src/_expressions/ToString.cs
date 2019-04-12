using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : Expression<string>, IExtension<object>
    {
        public ToString() : base(nameof(ToString)) { }

        public IExpression Value { get; set; }

        public IExpression Format { get; set; }

        protected override Constant<string> InvokeCore(IExpressionContext context)
        {
            var format = Format?.Invoke(context).ValueOrDefault<string>() ?? "{0}";

            if (context.TryPopExtensionInput(out object input))
            {
                return (Name, string.Format(CultureInfo.InvariantCulture, format, input), context);
            }
            else
            {
                var value = Value.Invoke(context).Value<object>();
                return (Name, string.Format(CultureInfo.InvariantCulture, format, value), context);
            }
        }
    }
}