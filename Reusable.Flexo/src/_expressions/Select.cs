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
            var result = new List<IConstant>();
            foreach (var (expression, i) in @this.Cast<IConstant>().Select((x, i) => (x, i)))
            {
                Expression.This.Push(expression);
                //var current = Selector.Invoke(context.PushThis(expression));
                var current = Selector.Invoke(context);
                context = current.Context;
                result.Add((Constant<object>)($"{Name.ToString()}-Item-{i}", current.Value, context));
            }

            return (Name, result, context);            
        }
    }

    public class Where : CollectionExtension<IEnumerable<IExpression>>
    {
        public Where([NotNull] ILogger logger) : base(logger, nameof(Where)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var result =
                @this
                    .Where(item =>
                    {
                        //Expression.This.Push(item);
                        return Predicate.Invoke(context).Value<bool>();
                        //return Predicate.Invoke(context.PushThis((Constant<object>)("Item", item, context))).Value<bool>();
                    })
                    .ToList();

            return (Name, result, context);
        }
    }

    public class ForEach : CollectionExtension<object>
    {
        public ForEach([NotNull] ILogger logger) : base(logger, nameof(ForEach)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            foreach (var item in @this)
            {
                var current = item.Invoke(context);
                foreach (var expression in Body)
                {
                    //expression.Invoke(context.PushThis(current));
                }
            }

            return (Name, default(object), context);
        }
    }
}