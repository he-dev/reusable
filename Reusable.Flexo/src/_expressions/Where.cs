using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

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
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, item)))
                {
                    return Predicate.Invoke().Value<bool>();
                }
            }).ToList();

            return (Name, result);
        }
    }
}