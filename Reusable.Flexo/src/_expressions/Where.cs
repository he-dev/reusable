using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IExpression>>
    {
        public Where([NotNull] ILogger<Where> logger) : base(logger, nameof(Where)) { }

        //protected override bool SuppressDebugView => true;
        
        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IEnumerable<IExpression> @this)
        {
            var result = @this.Where(item =>
            {
                using (BeginScope(ctx => ctx.SetItem(From<IExpressionMeta>.Select(m => m.This), item)))
                {
                    return Predicate.Invoke().Value<bool>();
                }
            }).ToList();

            return (Name, result);
        }
    }
}