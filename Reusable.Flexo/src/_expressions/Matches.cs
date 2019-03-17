using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public class Matches : PredicateExpression
    {
        public Matches() : base(nameof(Matches)) { }

        [DefaultValue(true)]
        public bool IgnoreCase { get; set; }

        [JsonRequired]
        public IExpression Value { get; set; }

        [JsonRequired]
        public object Pattern { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var value = Value.Invoke(context).ValueOrDefault<string>();
            if (value is null)
            {
                return (false, context);
            }
            else
            {
                var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return (Regex.IsMatch(value, GetPattern(context), options), context);
            }
        }

        private string GetPattern(IExpressionContext context)
        {
            switch (Pattern)
            {
                case string pattern: return pattern;
                case IExpression expression: return expression.Invoke(context).Value<string>();
                default:
                    throw DynamicException.Create
                    (
                        "UnsupportedPattern",
                        $"{Pattern.GetType().ToPrettyString()} is not supported. Expected '{nameof(String)}' or '{nameof(IExpression)}'."
                    );
            }
        }
    }
}