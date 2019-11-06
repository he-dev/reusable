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
        public Select() : base(default, nameof(Select)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression? Selector { get; set; }

        protected override IEnumerable<IExpression> InvokeAsValue(IImmutableContainer context)
        {
            var query =
                from item in This(context).Enabled()
                let selector = Selector ?? item
                select selector.Invoke(context, ImmutableContainer.Empty.SetItem(ExpressionContext.Item, item));

            return query.ToList();
        }
    }
}