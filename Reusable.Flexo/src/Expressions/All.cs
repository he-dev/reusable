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

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression? Predicate { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }
        
        protected override bool ComputeValue(IImmutableContainer context)
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true));//.Invoke(context);
            
            foreach (var item in This(context).Enabled())
            {
                var x = item.Invoke(context);

                var equal = predicate switch
                {
                    IConstant constant => context.GetEqualityComparerOrDefault(ComparerName).Equals(x.Value, constant.Value),
                    {} => predicate.Invoke(context, context.BeginScopeWithThisOuter(x)).Value<bool>(),
                    _ => EqualityComparer<bool>.Default.Equals(x.Value<bool>(), true)
                };

                if (!equal)
                {
                    return false;
                }
            }

            return true;
        }
    }
}