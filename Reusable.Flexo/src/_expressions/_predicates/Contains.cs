using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : CollectionExtension<bool>
    {
        public Contains(ILogger<Contains> logger) : base(logger, nameof(Contains)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Value { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var value = Value.Invoke(context).Value;
            var comparer = context.GetComparerOrDefault(Comparer);

            return (Name, @this.Any(x => comparer.Equals(value, x.Invoke(context).Value<object>())), context);
        }
    }
}