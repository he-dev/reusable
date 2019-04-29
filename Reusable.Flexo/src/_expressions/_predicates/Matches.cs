using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : ValueExtension<bool>
    {
        public Matches(ILogger<Matches> logger) : base(logger, nameof(Matches)) { }

        public bool IgnoreCase { get; set; } = true;

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        public IExpression Pattern { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context, IExpression @this)
        {
            var pattern = Pattern.Invoke(context).Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return (Name, Regex.IsMatch(@this.Invoke(context).Value<string>(), pattern, options), context);
        }
    }
}