using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public All(ILogger<All> logger) : base(logger, nameof(All)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; } //= Constant.True;

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                var predicate = (Predicate ?? Constant.True).Invoke(context).Value<bool>();
                return (Name, input.Cast<bool>().All(x => x == predicate), context);
            }
            else
            {
                var predicate = (Predicate ?? Constant.True).Invoke(context);
                var last = default(IConstant);
                foreach (var item in Values.Enabled())
                {
                    last = item.Invoke(last?.Context ?? context);
                    if (!EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate.Value<bool>()))
                    {
                        return (Name, false, context);
                    }
                }

                return (Name, true, last?.Context ?? context);
            }
        }
    }
}