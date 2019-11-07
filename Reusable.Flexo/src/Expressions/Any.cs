using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>
    {
        public Any() : base(default, nameof(Any)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression? Predicate { get; set; }
        
        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true)); //.Invoke();

            foreach (var item in This(context).Enabled())
            {
                var x = item.Invoke(context);

                var equal = predicate switch
                {
                    IConstant constant => context.GetEqualityComparerOrDefault(ComparerName).Equals(x.Value, constant.Value),
                    {} => predicate.Invoke(context, context.BeginScopeWithThisOuter(x)).Value<bool>(),
                    _ => EqualityComparer<bool>.Default.Equals(x.Value<bool>(), true)
                };

                if (equal)
                {
                    return true;
                }
            }

            return false;
        }
    }
}