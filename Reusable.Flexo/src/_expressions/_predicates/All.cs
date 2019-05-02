using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : CollectionExtension<bool>
    {
        public All(ILogger<All> logger) : base(logger, nameof(All)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var predicate = (Predicate ?? Constant.True).Invoke(context);
            foreach (var item in @this)
            {
                var current = item.Invoke(context);
                context = current.Context;
                if (!EqualityComparer<bool>.Default.Equals(current.Value<bool>(), predicate.Value<bool>()))
                {
                    return (Name, false, context);
                }
            }

            return (Name, true, context);
        }
    }
}