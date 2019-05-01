using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : CollectionExtension<IEnumerable<IExpression>>
    {
        public Select(ILogger<Select> logger) : base(logger, nameof(Select)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        [JsonRequired]
        public IExpression Selector { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var values = 
                @this
                    .Select(x => Selector.Invoke(context.PushThis((IConstant)x)))
                    .Select((x, i) => Constant.FromNameAndValue($"Item-{i}", x.Value))
                    .ToList();
            return (Name, values, context);
        }
    }

    //    public class FirstOrDefault : Expression
    //    {
    //        [JsonConstructor]
    //        public FirstOrDefault() : base(nameof(FirstOrDefault)) { }
    //
    //        public FirstOrDefault(string name, IExpressionContext context) : base(name, context) { }
    //
    //        public IExpression Predicate { get; set; }
    //
    //        public IEnumerable<IDictionary<string, IExpression>> Lookup { get; set; }
    //
    //        public override IExpression Invoke(IExpressionContext context)
    //        {
    //            foreach (var item in Lookup)
    //            {
    //                //if(Predicate)
    //            }
    //
    //            return Constant.Null;
    //        }
    //    }
}