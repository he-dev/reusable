using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNullOrEmpty : ValueExtension<bool>
    {
        public IsNullOrEmpty(ILogger<IsNullOrEmpty> logger) : base(logger, nameof(IsNullOrEmpty)) { }

        [JsonProperty("Left")]
        public override IExpression This { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            var value = (string)@this.Invoke().Value;
            return (Name, string.IsNullOrEmpty(value));
        }
    }
}