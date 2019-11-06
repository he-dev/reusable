using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>
    {
        public Any() : base(default, nameof(Any)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression Predicate { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true)); //.Invoke();

            foreach (var item in This(context).Enabled())
            {
                var x = item.Invoke(context);
                var y = predicate switch
                {
                    IConstant constant => constant.Invoke(context),
                    _ => predicate.Invoke(context, context.BeginScopeWithThisOuter(x))
                };

                if (EqualityComparer<bool>.Default.Equals(x.Value<bool>(), y.Value<bool>()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}