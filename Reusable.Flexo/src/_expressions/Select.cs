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

        public IExpression Selector { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore(IEnumerable<IExpression> @this)
        {
            var result = @this.Select(item =>
            {
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, item)))
                {
                    return (Selector ?? item).Invoke();
                }
            }).ToList();

            return (Name, result);
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