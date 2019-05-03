using System;
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

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IEnumerable<IExpression> @this)
        {
            //var debugView = CreateDebugView(this);
            var result = @this.Select(e =>
            {
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, e))) //.Set(Namespace, x => x.DebugView, debugView)))
                {
                    return Selector.Invoke();
                }
            }).ToList();

            //debugView.Value.Result = result;

            return (Name, result);
        }
    }

    public class Where : CollectionExtension<IEnumerable<IExpression>>
    {
        public Where([NotNull] ILogger logger) : base(logger, nameof(Where)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IEnumerable<IExpression> @this)
        {
            var result =
                @this
                    .Where(item =>
                    {
                        //Expression.This.Push(item);
                        return Predicate.Invoke().Value<bool>();
                        //return Predicate.Invoke(context.PushThis((Constant<object>)("Item", item, context))).Value<bool>();
                    })
                    .ToList();

            return (Name, result);
        }
    }

    public class ForEach : CollectionExtension<object>
    {
        public ForEach([NotNull] ILogger logger) : base(logger, nameof(ForEach)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore(IEnumerable<IExpression> @this)
        {
            /*
             
             foreach (var item in @this)
             {
                 using(Scope.New(ctx => ctx.SetItem("this", item"))) 
                 { 
                    foreach (var expression in Body)
                    {
                        expression.Invoke();
                    }
                 }
             }
             
             
             */
            foreach (var item in @this)
            {
                
                var itemValue = item.Invoke().Value;
                foreach (var expression in Body)
                {
                    //expression.Invoke(context.SetItem("this", itemValue));
                }
            }

            return (Name, default(object));
        }
    }

    // public abstract class Update : Expression<bool>
    // {
    //     protected Update([NotNull] ILogger logger, SoftString name) : base(logger, name) { }
    //
    //     [JsonProperty("Select")]
    //     public string Path { get; set; }
    //     
    //     public IEnumerable<IExpression> Values { get; set; }
    //
    //     protected override Constant<bool> InvokeCore(IImmutableSession context)
    //     {
    //         return default;
    //         // return
    //         //     Values is null
    //         //         ? InvokeCore(new[] { context["this"] }, context)
    //         //         : InvokeCore(Values.Invoke(context).Values<object>(), context);
    //     }
    //
    //     protected virtual Constant<bool> InvokeCore(IExpression value, IImmutableSession context)
    //     {
    //         throw new NotSupportedException();
    //     }
    //
    //     protected virtual Constant<bool> InvokeCore(IEnumerable<IExpression> values, IImmutableSession context)
    //     {
    //         throw new NotSupportedException();
    //     }        
    // }
    //
    // public class Append : Update
    // {
    //     public Append([NotNull] ILogger<Append> logger) : base(logger, nameof(Append)) { }
    //
    //     protected override Constant<bool> InvokeCore(IEnumerable<IExpression> values, IImmutableSession context)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }
}