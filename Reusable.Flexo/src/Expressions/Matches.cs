using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Matches : ScalarExtension<bool>
    {
        public Matches() : base(default, nameof(Matches)) { }

        public bool IgnoreCase { get; set; } = true;
        
        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        public IExpression Pattern { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var pattern = Pattern.Invoke(context).Value<string>();
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex.IsMatch(This(context).Invoke(context).Value<string>(), pattern, options);
        }
    }
}