using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class ForEach : CollectionExtension<object>
    {
        public ForEach([NotNull] ILogger<ForEach> logger) : base(logger, nameof(ForEach)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore(IEnumerable<IExpression> @this)
        {
            foreach (var item in @this)
            {
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.Item, item)))
                {
                    foreach (var expression in Body)
                    {
                        expression.Invoke();
                    }
                }
            }

            return (Name, default(object));
        }
    }
}