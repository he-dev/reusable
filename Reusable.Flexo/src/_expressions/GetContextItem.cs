using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
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

        protected Exception CreateKeyNotFoundException()
        {
            return DynamicException.Create("KeyNotFound", $"Expression-context does not contain an item with the key '{Key}'.");
        }
    }
    
    public class GetValue : GetContextItem<object>
    {
        public GetValue([NotNull] ILogger logger) : base(logger, nameof(GetValue)) { }
        
        protected override Constant<object> InvokeCore(IExpressionContext context)
        {
            return
                context.TryGetValue(Key, out var value)
                    ? (Key, value, context)
                    : throw CreateKeyNotFoundException();
        }
    }
    
    public class GetCollection : GetContextItem<IEnumerable<object>>
    {
        public GetCollection([NotNull] ILogger logger) : base(logger, nameof(GetCollection)) { }
        
        protected override Constant<IEnumerable<object>> InvokeCore(IExpressionContext context)
        {
            return
                context.TryGetValue(Key, out var value) && value is IEnumerable<object> collection
                    ? (Key, collection, context)
                    : throw CreateKeyNotFoundException();
        }
    }
}