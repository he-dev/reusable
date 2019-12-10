using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : Extension<string, bool>, IFilter
    {
        public Matches() : base(default) { }

        public IExpression? Value
        {
            set => Arg = value;
        }

        public bool IgnoreCase { get; set; } = true;

        [JsonRequired]
        [JsonProperty("Pattern")]
        public IExpression? Matcher { get; set; } = default!;

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            var input = GetArg(context).AsEnumerable<string>().Single();
            var pattern = Matcher!.Invoke(context).AsEnumerable<string>().Single();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex.IsMatch(input, pattern, options);
        }
    }
}