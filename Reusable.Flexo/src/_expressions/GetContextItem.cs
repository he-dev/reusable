using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Gets an item from the context. Calls Invoke if it's an expression, otherwise it's wrapped in a Constant.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public abstract class GetContextItem<T> : Expression<T>
    {
        protected GetContextItem(ILogger logger, string name) : base(logger, name) { }

        [JsonRequired]
        public string Key { get; set; }

        protected Constant<TItem> FindItem<TItem>(IImmutableSession context)
        {
            if (context.TryGetValue(Key, out var item))
            {
                return (Key, (TItem)item, context);
            }
            else
            {
                throw DynamicException.Create("ItemNotFound", $"Could not find item with the key '{Key}'.");
            }
        }
    }

    public class GetValue : GetContextItem<object>
    {
        public GetValue([NotNull] ILogger logger) : base(logger, nameof(GetValue)) { }

        protected override Constant<object> InvokeCore(IImmutableSession context)
        {
            return FindItem<object>(context);
        }
    }

    public class GetCollection : GetContextItem<IEnumerable<object>>
    {
        public GetCollection([NotNull] ILogger logger) : base(logger, nameof(GetCollection)) { }

        protected override Constant<IEnumerable<object>> InvokeCore(IImmutableSession context)
        {
            return FindItem<IEnumerable<object>>(context);
        }
    }
}