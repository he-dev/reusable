using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression, IExtension<IEnumerable<IConstant>>
    {
        public Any(ILogger<Any> logger) : base(logger, nameof(Any)) { }

        [This]
        public List<IExpression> Values { get; set; } //= new List<IExpression>();

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            var @this = context.PopThis().Invoke(context).Value<IEnumerable<IExpression>>();

//            if (context.TryPopExtensionInput(out IEnumerable<object> input))
//            {
//                foreach (var item in input)
//                {
//                    context.PushExtensionInput(item);
//                    var predicateResult = (Predicate ?? Constant.True).Invoke(context).Value<bool>();
//                    if (EqualityComparer<bool>.Default.Equals(predicateResult, true))
//                    {
//                        return (Name, true, context);
//                    }
//                }
//            }
//            else
            {
                //var p = Predicate ?? new IsEqual(default);
                
                var last = default(IConstant);
                foreach (var item in @this.Enabled())
                {
                    var value = item.Invoke(context);
                    context.PushThis(value);
                    var predicate = (Predicate ?? Constant.True);

                    if (predicate is IConstant)
                    {
                        last = value;
                        if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate.Invoke(context).Value<bool>()))
                        {
                            return (Name, true, last.Context);
                        }   
                    }
                    else
                    {

                        last = value; //.Invoke(predicate.Context);
                        if (EqualityComparer<bool>.Default.Equals(predicate.Invoke(context).Value<bool>(), true))
                            //if (EqualityComparer<bool>.Default.Equals(predicate.Value<bool>(), true))
                        {
                            return (Name, true, last.Context);
                        }
                    }
                }

                return (Name, false, context);
            }
        }
    }
}