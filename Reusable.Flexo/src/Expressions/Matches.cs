using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : ScalarExtension<bool>, IFilter
    {
        public Matches() : base(default) { }

        public IExpression Value
        {
            get => Arg;
            set => Arg = value;
        }
        
        public bool IgnoreCase { get; set; } = true;

        [JsonProperty("Pattern")]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var input = GetArg(context).Invoke(context).Value<string>();
            var pattern = Matcher.Invoke(context).Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex.IsMatch(input, pattern, options);
        }

    }
}