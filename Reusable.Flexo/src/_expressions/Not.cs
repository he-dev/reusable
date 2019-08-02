using System.Collections.Generic;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : ValueExpressionExtension<bool>
    {
        public Not(ILogger<Not> logger) : base(logger, nameof(Not)) { }
        
        public IExpression Value { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        protected override Constant<bool> InvokeCore()
        {
            return (Name, !Value.Invoke().Value<bool>());            
        }
    }
}