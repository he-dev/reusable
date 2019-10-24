using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : CollectionExtension<bool>
    {
        public Overlaps(ILogger<Overlaps> logger) : base(logger, nameof(Overlaps)) { }
        
        public IEnumerable<IExpression> Values { get => This; set => This = value; }

        [JsonRequired]
        public List<IExpression> With { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var with = With.Enabled().Invoke().Values<object>();
            var comparer = Scope.GetComparerOrDefault(Comparer);
            var values = Values.Enabled().Invoke().Values<object>();
            return (Name, values.Intersect(with, comparer).Any());
        }
    }
}