using System.Text.RegularExpressions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : PredicateExpression, IExtension<string>
    {
        public Matches(ILogger<Matches> logger) : base(logger, nameof(Matches)) { }

        public bool IgnoreCase { get; set; } = true;

        public IExpression Value { get; set; }

        public IExpression Pattern { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var pattern = Pattern.Invoke(context).Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            if (context.TryPopExtensionInput(out string input))
            {
                return (Name, Regex.IsMatch(input, pattern, options), context);
            }
            else
            {
                var value = Value.Invoke(context).Value<string>();
                return (Name, Regex.IsMatch(value, pattern, options), context);
            }
        }
    }
}