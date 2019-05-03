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

        protected override Constant<bool> InvokeCore(IEnumerable<IExpression> @this)
        {
            var value = Value.Invoke().Value;
            var comparer = Scope.GetComparerOrDefault(Comparer);
            return (Name, @this.Any(x => comparer.Equals(value, x.Invoke().Value<object>())));
        }
    }
    
    [PublicAPI]
    public class In : ValueExtension<bool>
    {
        public In(ILogger<In> logger) : base(logger, nameof(In)) { }

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        public IEnumerable<IExpression> Values { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            var value = @this.Invoke().Value;
            var comparer = Scope.GetComparerOrDefault(Comparer);

            return (Name, Values.Enabled().Any(x => comparer.Equals(value, x.Invoke().Value)));
        }
    }
}