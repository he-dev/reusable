using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsNull : ValueExtension<bool>
    {
        public IsNull(ILogger<IsNull> logger) : base(logger, nameof(IsNull)) { }

        [JsonProperty("Left")]
        public override IExpression This { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            var value = @this.Invoke().Value;
            return (Name, value is null);
        }
    }
}