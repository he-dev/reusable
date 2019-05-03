using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : ValueExtension<string>
    {
        public ToString(ILogger<ToString> logger) : base(logger, nameof(ToString)) { }

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        public IExpression Format { get; set; }

        protected override Constant<string> InvokeCore(IExpression @this)
        {
            // Scope.This
            // Scope.Find("in") --> global object
            var format = Format?.Invoke().ValueOrDefault<string>() ?? "{0}";
            return (Name, string.Format(CultureInfo.InvariantCulture, format, @this.Invoke().Value));
        }
    }
}