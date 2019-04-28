using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public Contains(ILogger<Contains> logger) : base(logger, nameof(Contains)) { }

        [This]
        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Value { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            var @this = context.This().Invoke(context).Value<IEnumerable<object>>();
            
            var value = Value.Invoke(context).Value;
            var comparer = context.GetComparerOrDefault(Comparer);
            
            return (Name, @this.Any(x => comparer.Equals(value, x)), context);

            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                return (Name, input.Any(x => comparer.Equals(value, x)), context);
            }
            else
            {
                var values = Values.Enabled().Select(x => x.Invoke(context).Value);
                return (Name, values.Any(x => comparer.Equals(value, x)), context);
            }
        }
    }
}