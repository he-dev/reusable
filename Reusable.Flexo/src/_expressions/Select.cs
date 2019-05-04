using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : CollectionExtension<IEnumerable<IExpression>>
    {
        public Select(ILogger<Select> logger) : base(logger, nameof(Select)) { }

        protected override bool SuppressOwnDebugView => true;

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Selector { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IEnumerable<IExpression> @this)
        {
            var result = @this.Select(item =>
            {
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, item)))
                {
                    return (Selector ?? item).Invoke();
                }
            }).ToList();

            return (Name, result);
        }
    }

    
}