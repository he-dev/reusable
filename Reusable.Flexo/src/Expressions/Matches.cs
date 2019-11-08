using System.Text.RegularExpressions;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Matches : ScalarExtension<bool>
    {
        public Matches() : base(default) { }

        public bool IgnoreCase { get; set; } = true;
        
        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        public IExpression Pattern { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var input = This(context).Invoke(context).Value<string>();
            var pattern = Pattern.Invoke(context).Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex.IsMatch(input, pattern, options);
        }
    }
}