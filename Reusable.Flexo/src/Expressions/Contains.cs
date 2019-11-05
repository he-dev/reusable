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

        public IEnumerable<IExpression> Values { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IExpression Value { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var value = Value.Invoke(TODO).Value;
            var comparer = Scope.GetComparerOrDefault(Comparer);
            return (Name, Values.Any(x => comparer.Equals(value, x.Invoke(TODO).Value<object>())));
        }
    }

    [PublicAPI]
    public class In : ScalarExtension<bool>
    {
        public In(ILogger<In> logger) : base(logger, nameof(In)) { }

        public IExpression Value { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IEnumerable<IExpression> Values { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var value = Value.Invoke(TODO).Value;
            var comparer = Scope.GetComparerOrDefault(Comparer);

            return (Name, Values.Enabled().Any(x => comparer.Equals(value, x.Invoke(TODO).Value)));
        }
    }
}