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

        public IExpression Predicate { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true));//.Invoke(context);
            foreach (var item in This(context).Enabled())
            {
                var x = item.Invoke(context);
                var y = predicate switch
                {
                    IConstant constant => constant.Invoke(context),
                    _ => predicate.Invoke(context, context.BeginScopeWithThisOuter(x))
                };

                if (EqualityComparer<bool>.Default.Equals(x.Value<bool>(), !y.Value<bool>()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}