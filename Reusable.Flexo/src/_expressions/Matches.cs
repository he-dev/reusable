using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Matches : PredicateExpression
    {
        protected Matches() : base(nameof(Matches)) { }

        [DefaultValue(true)]
        public bool IgnoreCase { get; set; } = true;

        [JsonRequired]
        public IExpression Expression { get; set; }

        [JsonRequired]
        public string Pattern { get; set; }

        protected override InvokeResult<bool> Calculate(IExpressionContext context)
        {
            var x = Expression.InvokeWithValidation(context).Value<string>();            
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            var result = !(x is null) && Regex.IsMatch(x, Pattern, options);
            return InvokeResult.From(result, context);
        }
    }
}