using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

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

        
        public  IEnumerable<IExpression> Values { get => This; set => This = value; }

        public IExpression Selector { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore()
        {
            var result = Values.Enabled().Select(item =>
            {
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.Item, item)))
                {
                    return (Selector ?? item).Invoke();
                }
            }).ToList();

            return (Name, result);
        }
    }
}