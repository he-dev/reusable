using System.Collections.Generic;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : ValueExtension<bool>
    {
        public Not(ILogger<Not> logger) : base(logger, nameof(Not)) { }

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            return (Name, !@this.Invoke().Value<bool>());            
        }
    }
}